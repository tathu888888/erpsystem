' FrmCostingVariance.vb
Option Strict On
Option Infer On
Option Explicit On

Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports MySqlConnector

Public Class FrmCostingVariance
    Inherits Form

    ' =========================
    ' ★ 設定（あなたの環境に合わせて変更）
    ' =========================
    Private Const CONN_STR As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;User Id=root;Password=your_password;SslMode=None;AllowUserVariables=True;"

    ' inventory_ledger の ref_type（あなたの実装に合わせて）
    Private Const REF_PROD_ISSUE As String = "PRODUCTION_ISSUE"   ' 材料払出
    Private Const REF_PROD_RECEIPT As String = "PRODUCTION_RECEIPT" ' 完成入庫（あれば）

    ' 標準原価の取得元（あなたの設計に合わせる）
    ' 推奨: item_cost_master(item_id, standard_cost) を作る
    ' なければ item_master.standard_cost 等に合わせる
    Private Const STD_COST_SOURCE As String = "item_cost_master" ' "item_master" にするなど

    ' =========================
    ' UI Controls
    ' =========================
    Private txtProductionId As TextBox
    Private btnLoad As Button
    Private lblHeader As Label

    Private lblStdTotal As Label
    Private lblActTotal As Label
    Private lblVarTotal As Label

    Private tab As TabControl
    Private gridStd As DataGridView
    Private gridAct As DataGridView
    Private gridVar As DataGridView
    Private gridGL As DataGridView

    Private status As StatusStrip
    Private statusLabel As ToolStripStatusLabel

    ' Data
    Private dtStd As DataTable
    Private dtAct As DataTable
    Private dtVar As DataTable
    Private dtGL As DataTable

    Public Sub New()
        Me.Text = "Costing / Variance（原価計算・差異・仕訳プレビュー）"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Width = 1280
        Me.Height = 760

        BuildUI()
    End Sub

    Private Sub BuildUI()
        Me.SuspendLayout()

        lblHeader = New Label() With {
            .Text = "原価計算・差異（Costing / Variance）",
            .AutoSize = False,
            .Dock = DockStyle.Top,
            .Height = 48,
            .Font = New Font("Yu Gothic UI", 16, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(12, 0, 0, 0)
        }
        Me.Controls.Add(lblHeader)

        Dim topPanel As New Panel() With {.Dock = DockStyle.Top, .Height = 64, .Padding = New Padding(12, 8, 12, 8)}
        Me.Controls.Add(topPanel)

        Dim lblPid As New Label() With {.Text = "production_id:", .AutoSize = True, .Location = New Point(8, 12)}
        topPanel.Controls.Add(lblPid)

        txtProductionId = New TextBox() With {.Width = 180, .Location = New Point(110, 8)}
        topPanel.Controls.Add(txtProductionId)

        btnLoad = New Button() With {.Text = "ロード / 計算", .Width = 120, .Height = 30, .Location = New Point(300, 6)}
        AddHandler btnLoad.Click, AddressOf btnLoad_Click
        topPanel.Controls.Add(btnLoad)

        Dim summaryPanel As New Panel() With {.Dock = DockStyle.Top, .Height = 56, .Padding = New Padding(12, 4, 12, 4)}
        Me.Controls.Add(summaryPanel)

        lblStdTotal = New Label() With {.AutoSize = False, .Width = 360, .Height = 44, .Location = New Point(12, 6), .Font = New Font("Yu Gothic UI", 11, FontStyle.Bold)}
        lblActTotal = New Label() With {.AutoSize = False, .Width = 360, .Height = 44, .Location = New Point(390, 6), .Font = New Font("Yu Gothic UI", 11, FontStyle.Bold)}
        lblVarTotal = New Label() With {.AutoSize = False, .Width = 360, .Height = 44, .Location = New Point(768, 6), .Font = New Font("Yu Gothic UI", 11, FontStyle.Bold)}

        summaryPanel.Controls.Add(lblStdTotal)
        summaryPanel.Controls.Add(lblActTotal)
        summaryPanel.Controls.Add(lblVarTotal)

        tab = New TabControl() With {.Dock = DockStyle.Fill}
        Me.Controls.Add(tab)

        gridStd = CreateGrid()
        gridAct = CreateGrid()
        gridVar = CreateGrid()
        gridGL = CreateGrid()

        tab.TabPages.Add(MakeTab("📘 Standard（標準BOM原価）", gridStd))
        tab.TabPages.Add(MakeTab("📗 Actual（実際消費原価）", gridAct))
        tab.TabPages.Add(MakeTab("📊 Variance（差異）", gridVar))
        tab.TabPages.Add(MakeTab("💰 GL Preview（仕訳プレビュー）", gridGL))

        status = New StatusStrip()
        statusLabel = New ToolStripStatusLabel("Ready")
        status.Items.Add(statusLabel)
        Me.Controls.Add(status)

        UpdateSummary(0D, 0D, 0D)

        Me.ResumeLayout()
    End Sub

    Private Function MakeTab(title As String, grid As DataGridView) As TabPage
        Dim tp As New TabPage(title)
        grid.Dock = DockStyle.Fill
        tp.Controls.Add(grid)
        Return tp
    End Function

    Private Function CreateGrid() As DataGridView
        Dim g As New DataGridView() With {
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
            .RowHeadersVisible = False,
            .Dock = DockStyle.Fill,
            .BackgroundColor = Color.White
        }
        Return g
    End Function

    Private Sub btnLoad_Click(sender As Object, e As EventArgs)
        Dim productionId As Long
        If Not Long.TryParse(txtProductionId.Text.Trim(), productionId) OrElse productionId <= 0 Then
            MessageBox.Show("production_id を正しく入力してください。", "Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            statusLabel.Text = "Loading..."
            Application.DoEvents()

            ' 1) 標準（BOMベース）※production_id -> bom_id を辿れる想定 or production_order_header がある想定
            ' ここは環境で差が出るので 2段階：
            '   A) production_order_header から bom_id を取得
            '   B) なければ production_id を bom_id とみなす（暫定運用）
            Dim bomId As Long = GetBomIdByProductionId(productionId)
            If bomId <= 0 Then bomId = productionId

            dtStd = LoadStandardCostByBom(bomId)
            gridStd.DataSource = dtStd
            FormatMoneyColumns(gridStd, New String() {"std_cost", "std_amount"})

            ' 2) 実績（inventory_ledger）
            dtAct = LoadActualConsumption(productionId)
            gridAct.DataSource = dtAct
            FormatMoneyColumns(gridAct, New String() {"actual_unit_cost", "actual_amount"})

            ' 3) 差異（標準×実績の突合）
            dtVar = BuildVariance(dtStd, dtAct)
            gridVar.DataSource = dtVar
            FormatMoneyColumns(gridVar, New String() {"std_cost", "std_amount", "actual_unit_cost", "actual_amount", "qty_variance", "price_variance", "total_variance"})
            ColorVariance(gridVar, "total_variance")

            ' 4) 仕訳プレビュー（gl_journal_line）
            dtGL = LoadGlPreview(productionId)
            gridGL.DataSource = dtGL
            FormatMoneyColumns(gridGL, New String() {"debit", "credit", "net"})
            ColorVariance(gridGL, "net") ' net は残高として色付け

            ' totals
            Dim stdTotal As Decimal = SumColumn(dtStd, "std_amount")
            Dim actTotal As Decimal = SumColumn(dtAct, "actual_amount")
            Dim varTotal As Decimal = actTotal - stdTotal
            UpdateSummary(stdTotal, actTotal, varTotal)

            statusLabel.Text = "Loaded"
        Catch ex As Exception
            statusLabel.Text = "Error"
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' =========================
    ' DB Access
    ' =========================
    Private Function OpenConn() As MySqlConnection
        Dim conn As New MySqlConnection(CONN_STR)
        conn.Open()
        Return conn
    End Function

    Private Function ExecDataTable(sql As String, params As Dictionary(Of String, Object)) As DataTable
        Using conn = OpenConn()
            Using cmd As New MySqlCommand(sql, conn)
                For Each kv In params
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value)
                Next
                Using adp As New MySqlDataAdapter(cmd)
                    Dim dt As New DataTable()
                    adp.Fill(dt)
                    Return dt
                End Using
            End Using
        End Using
    End Function

    Private Function GetBomIdByProductionId(productionId As Long) As Long
        ' production_order_header がある場合:
        ' production_order_header(production_id, bom_id)
        ' 無ければ 0 を返す（呼び出し側で暫定bomId=productionIdにする）
        Dim sql = "
SELECT COALESCE(poh.bom_id, 0) AS bom_id
FROM production_order_header poh
WHERE poh.production_id = @production_id
LIMIT 1;
"
        Try
            Dim dt = ExecDataTable(sql, New Dictionary(Of String, Object) From {
                {"@production_id", productionId}
            })
            If dt.Rows.Count = 0 Then Return 0
            Return Convert.ToInt64(dt.Rows(0)("bom_id"))
        Catch
            ' テーブル無い/列無い場合でも落とさない
            Return 0
        End Try
    End Function

    Private Function LoadStandardCostByBom(bomId As Long) As DataTable
        Dim sql As String

        If STD_COST_SOURCE = "item_cost_master" Then
            sql = "
SELECT
    bd.component_item_id AS item_id,
    im.item_code,
    im.item_name,
    bd.quantity_required AS std_qty,
    COALESCE(ic.standard_cost, 0) AS std_cost,
    (bd.quantity_required * COALESCE(ic.standard_cost, 0)) AS std_amount
FROM bom_detail bd
JOIN item_master im ON im.item_id = bd.component_item_id
LEFT JOIN item_cost_master ic ON ic.item_id = im.item_id
WHERE bd.bom_id = @bom_id
ORDER BY im.item_code;
"
        Else
            ' item_master に標準単価がある想定（列名は適宜変更）
            sql = "
SELECT
    bd.component_item_id AS item_id,
    im.item_code,
    im.item_name,
    bd.quantity_required AS std_qty,
    COALESCE(im.standard_cost, 0) AS std_cost,
    (bd.quantity_required * COALESCE(im.standard_cost, 0)) AS std_amount
FROM bom_detail bd
JOIN item_master im ON im.item_id = bd.component_item_id
WHERE bd.bom_id = @bom_id
ORDER BY im.item_code;
"
        End If

        Return ExecDataTable(sql, New Dictionary(Of String, Object) From {
            {"@bom_id", bomId}
        })
    End Function

    Private Function LoadActualConsumption(productionId As Long) As DataTable
        ' inventory_ledger:
        '   qty_delta: 払出ならマイナスになりがち。ここでは消費量として ABS で表示する
        '   amount_delta: 払出金額（マイナスの場合あり）
        Dim sql = "
SELECT
    il.item_id,
    im.item_code,
    im.item_name,
    ABS(SUM(il.qty_delta)) AS actual_qty,
    ABS(SUM(il.amount_delta)) AS actual_amount,
    CASE WHEN ABS(SUM(il.qty_delta)) = 0 THEN 0
         ELSE (ABS(SUM(il.amount_delta)) / ABS(SUM(il.qty_delta)))
    END AS actual_unit_cost
FROM inventory_ledger il
JOIN item_master im ON im.item_id = il.item_id
WHERE il.ref_type = @ref_type
  AND il.ref_id = @production_id
GROUP BY il.item_id, im.item_code, im.item_name
ORDER BY im.item_code;
"
        Return ExecDataTable(sql, New Dictionary(Of String, Object) From {
            {"@ref_type", REF_PROD_ISSUE},
            {"@production_id", productionId}
        })
    End Function

    Private Function LoadGlPreview(productionId As Long) As DataTable
        ' gl_journal_line:
        ' ref_type='PRODUCTION' 等あなたの運用に合わせて変更
        ' ここは「実際に起票済みの仕訳」をプレビューします。
        Dim sql = "
SELECT
    jl.account_code,
    am.account_name,
    SUM(jl.debit)  AS debit,
    SUM(jl.credit) AS credit,
    (SUM(jl.debit) - SUM(jl.credit)) AS net
FROM gl_journal_line jl
LEFT JOIN account_master am ON am.account_code = jl.account_code
WHERE jl.ref_type = 'PRODUCTION'
  AND jl.ref_id = @production_id
GROUP BY jl.account_code, am.account_name
ORDER BY jl.account_code;
"
        Try
            Return ExecDataTable(sql, New Dictionary(Of String, Object) From {
                {"@production_id", productionId}
            })
        Catch
            ' テーブル/列が未整備でも落とさない
            Dim dt As New DataTable()
            dt.Columns.Add("account_code", GetType(String))
            dt.Columns.Add("account_name", GetType(String))
            dt.Columns.Add("debit", GetType(Decimal))
            dt.Columns.Add("credit", GetType(Decimal))
            dt.Columns.Add("net", GetType(Decimal))
            Return dt
        End Try
    End Function

    ' =========================
    ' Variance Build（標準×実績）
    ' =========================
    Private Function BuildVariance(stdDt As DataTable, actDt As DataTable) As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("item_id", GetType(Long))
        dt.Columns.Add("item_code", GetType(String))
        dt.Columns.Add("item_name", GetType(String))

        dt.Columns.Add("std_qty", GetType(Decimal))
        dt.Columns.Add("std_cost", GetType(Decimal))
        dt.Columns.Add("std_amount", GetType(Decimal))

        dt.Columns.Add("actual_qty", GetType(Decimal))
        dt.Columns.Add("actual_unit_cost", GetType(Decimal))
        dt.Columns.Add("actual_amount", GetType(Decimal))

        dt.Columns.Add("qty_variance", GetType(Decimal))    ' (actual_qty - std_qty) * std_cost
        dt.Columns.Add("price_variance", GetType(Decimal))  ' (actual_unit_cost - std_cost) * actual_qty
        dt.Columns.Add("total_variance", GetType(Decimal))  ' actual_amount - std_amount

        ' index actual by item_id
        Dim actMap As New Dictionary(Of Long, DataRow)()
        For Each r As DataRow In actDt.Rows
            Dim id = Convert.ToInt64(r("item_id"))
            actMap(id) = r
        Next

        ' std items
        For Each s As DataRow In stdDt.Rows
            Dim itemId = Convert.ToInt64(s("item_id"))
            Dim code = Convert.ToString(s("item_code"))
            Dim name = Convert.ToString(s("item_name"))

            Dim stdQty = ToDec(s("std_qty"))
            Dim stdCost = ToDec(s("std_cost"))
            Dim stdAmt = ToDec(s("std_amount"))

            Dim actQty As Decimal = 0D
            Dim actAmt As Decimal = 0D
            Dim actUnit As Decimal = 0D

            If actMap.ContainsKey(itemId) Then
                Dim a = actMap(itemId)
                actQty = ToDec(a("actual_qty"))
                actAmt = ToDec(a("actual_amount"))
                actUnit = ToDec(a("actual_unit_cost"))
            End If

            Dim qtyVar = (actQty - stdQty) * stdCost
            Dim priceVar = (actUnit - stdCost) * actQty
            Dim totalVar = actAmt - stdAmt

            dt.Rows.Add(itemId, code, name,
                        stdQty, stdCost, stdAmt,
                        actQty, actUnit, actAmt,
                        qtyVar, priceVar, totalVar)
        Next

        ' actual-only items（代替材・未登録品など）
        For Each a As DataRow In actDt.Rows
            Dim itemId = Convert.ToInt64(a("item_id"))
            If stdDt.Select("item_id=" & itemId.ToString()).Length > 0 Then Continue For

            Dim code = Convert.ToString(a("item_code"))
            Dim name = Convert.ToString(a("item_name"))
            Dim actQty = ToDec(a("actual_qty"))
            Dim actAmt = ToDec(a("actual_amount"))
            Dim actUnit = ToDec(a("actual_unit_cost"))

            ' 標準が無い → 全額差異扱い
            dt.Rows.Add(itemId, code, name,
                        0D, 0D, 0D,
                        actQty, actUnit, actAmt,
                        actQty * 0D, (actUnit - 0D) * actQty, actAmt)
        Next

        Return dt
    End Function

    ' =========================
    ' Helpers
    ' =========================
    Private Sub UpdateSummary(stdTotal As Decimal, actTotal As Decimal, varTotal As Decimal)
        lblStdTotal.Text = $"標準原価 合計: {stdTotal:N0}"
        lblActTotal.Text = $"実際原価 合計: {actTotal:N0}"
        lblVarTotal.Text = $"総差異: {varTotal:N0}"

        ' 差異がプラス（実際>標準）なら赤、マイナスなら青っぽく
        If varTotal > 0D Then
            lblVarTotal.ForeColor = Color.DarkRed
        ElseIf varTotal < 0D Then
            lblVarTotal.ForeColor = Color.DarkBlue
        Else
            lblVarTotal.ForeColor = Color.Black
        End If
    End Sub

    Private Function SumColumn(dt As DataTable, col As String) As Decimal
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return 0D
        Dim sum As Decimal = 0D
        For Each r As DataRow In dt.Rows
            sum += ToDec(r(col))
        Next
        Return sum
    End Function

    Private Function ToDec(v As Object) As Decimal
        If v Is Nothing OrElse v Is DBNull.Value Then Return 0D
        Dim d As Decimal
        If Decimal.TryParse(v.ToString(), d) Then Return d
        Return 0D
    End Function

    Private Sub FormatMoneyColumns(grid As DataGridView, colNames As IEnumerable(Of String))
        For Each Name In colNames
            If grid.Columns.Contains(Name) Then
                grid.Columns(Name).DefaultCellStyle.Format = "N2"
                grid.Columns(Name).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            End If
        Next

        ' 数量っぽい列も整形
        For Each qtyName In New String() {"std_qty", "actual_qty"}
            If grid.Columns.Contains(qtyName) Then
                grid.Columns(qtyName).DefaultCellStyle.Format = "N3"
                grid.Columns(qtyName).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            End If
        Next
    End Sub

    Private Sub ColorVariance(grid As DataGridView, varianceColumn As String)
        If Not grid.Columns.Contains(varianceColumn) Then Return

        AddHandler grid.CellFormatting,
            Sub(sender As Object, e As DataGridViewCellFormattingEventArgs)
                If e.RowIndex < 0 Then Return
                If grid.Columns(e.ColumnIndex).Name <> varianceColumn Then Return
                If e.Value Is Nothing Then Return

                Dim v As Decimal
                If Not Decimal.TryParse(e.Value.ToString(), v) Then Return

                If v > 0D Then
                    e.CellStyle.ForeColor = Color.DarkRed
                ElseIf v < 0D Then
                    e.CellStyle.ForeColor = Color.DarkBlue
                Else
                    e.CellStyle.ForeColor = Color.Black
                End If
            End Sub
    End Sub

End Class