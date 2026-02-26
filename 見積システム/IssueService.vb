Imports MySql.Data.MySqlClient

Public Class IssueService

    ' ==========
    ' SAVE: ヘッダ＋明細を保存（posted=0の状態）
    ' ==========
    '    Public Function SaveDraft(header As IssueHeader, lines As List(Of IssueLine)) As Long
    '        If header Is Nothing Then Throw New ArgumentNullException(NameOf(header))
    '        If lines Is Nothing OrElse lines.Count = 0 Then Throw New Exception("明細がありません。")

    '        ValidateLinesBasic(lines)

    '        Using cn = Db.OpenConnection()
    '            Using tx = cn.BeginTransaction()
    '                Try
    '                    Dim issueId As Long

    '                    If header.IssueId <= 0 Then
    '                        Dim sqlInsH =
    '"INSERT INTO issue_header(issue_date, ref_type, ref_id, warehouse_id, memo, posted, created_by)
    ' VALUES(@issue_date, @ref_type, @ref_id, @warehouse_id, @memo, 0, @created_by);
    'SELECT LAST_INSERT_ID();"
    '                        Using cmd = Db.CreateCmd(sqlInsH, cn, tx)
    '                            Db.Param(cmd, "@issue_date", header.IssueDate)
    '                            Db.Param(cmd, "@ref_type", header.RefType)
    '                            Db.Param(cmd, "@ref_id", header.RefId)
    '                            Db.Param(cmd, "@warehouse_id", header.WarehouseId)
    '                            Db.Param(cmd, "@memo", header.Memo)
    '                            Db.Param(cmd, "@created_by", header.CreatedBy)
    '                            issueId = Convert.ToInt64(cmd.ExecuteScalar())
    '                        End Using
    '                    Else
    '                        issueId = header.IssueId
    '                        ' posted=0 のみ更新可能にする
    '                        Dim sqlUpdH =
    '"UPDATE issue_header
    ' SET issue_date=@issue_date, ref_type=@ref_type, ref_id=@ref_id, warehouse_id=@warehouse_id, memo=@memo
    ' WHERE issue_id=@issue_id AND posted=0;"
    '                        Using cmd = Db.CreateCmd(sqlUpdH, cn, tx)
    '                            Db.Param(cmd, "@issue_date", header.IssueDate)
    '                            Db.Param(cmd, "@ref_type", header.RefType)
    '                            Db.Param(cmd, "@ref_id", header.RefId)
    '                            Db.Param(cmd, "@warehouse_id", header.WarehouseId)
    '                            Db.Param(cmd, "@memo", header.Memo)
    '                            Db.Param(cmd, "@issue_id", issueId)
    '                            Dim n = cmd.ExecuteNonQuery()
    '                            If n = 0 Then Throw New Exception("確定済み、または存在しないため更新できません。")
    '                        End Using

    '                        ' 明細は一旦全削除→入れ直し（簡単・安全）
    '                        Using cmdDel = Db.CreateCmd("DELETE FROM issue_line WHERE issue_id=@issue_id;", cn, tx)
    '                            Db.Param(cmdDel, "@issue_id", issueId)
    '                            cmdDel.ExecuteNonQuery()
    '                        End Using
    '                    End If

    '                    Dim sqlInsL =
    '"INSERT INTO issue_line(issue_id, item_id, qty_pieces, lot_id, lot_unit_id, reason_code, note, alloc_key)
    ' VALUES(@issue_id, @item_id, @qty_pieces, @lot_id, @lot_unit_id, @reason_code, @note, @alloc_key);"

    '                    For Each ln In lines
    '                        Using cmd = Db.CreateCmd(sqlInsL, cn, tx)
    '                            Db.Param(cmd, "@issue_id", issueId)
    '                            Db.Param(cmd, "@item_id", ln.ItemId)
    '                            Db.Param(cmd, "@qty_pieces", ln.QtyPieces)
    '                            Db.Param(cmd, "@lot_id", ln.LotId)
    '                            Db.Param(cmd, "@lot_unit_id", ln.LotUnitId)
    '                            Db.Param(cmd, "@reason_code", ln.ReasonCode)
    '                            Db.Param(cmd, "@note", ln.Note)
    '                            Db.Param(cmd, "@alloc_key", ln.AllocKey)
    '                            cmd.ExecuteNonQuery()
    '                        End Using
    '                    Next

    '                    tx.Commit()
    '                    Return issueId
    '                Catch
    '                    tx.Rollback()
    '                    Throw
    '                End Try
    '            End Using
    '        End Using
    '    End Function


    '    ' ==========
    '    ' POST: 確定（在庫台帳・在庫数量・個体ステータスを更新）
    '    ' ==========
    '    Public Sub Post(issueId As Long, postedBy As String)
    '        If issueId <= 0 Then Throw New Exception("issueIdが不正です。")

    '        Using cn = Db.OpenConnection()
    '            Using tx = cn.BeginTransaction(IsolationLevel.ReadCommitted)
    '                Try
    '                    ' 1) ヘッダをロックして posted=0 を確認
    '                    Dim header = GetHeaderForUpdate(issueId, cn, tx)
    '                    If header Is Nothing Then Throw New Exception("伝票が見つかりません。")
    '                    If header.Posted Then Throw New Exception("すでに確定済みです。")

    '                    ' 2) 明細取得
    '                    Dim lines = GetLines(issueId, cn, tx)
    '                    If lines.Count = 0 Then Throw New Exception("明細がありません。")

    '                    ValidateLinesBasic(lines)

    '                    ' 3) ロット/個体をロック（FOR UPDATE）
    '                    LockLotsAndUnits(lines, cn, tx)

    '                    ' 4) 在庫不足チェック（lot.qty_on_hand_pieces >= qty）
    '                    ValidateOnHand(lines, cn, tx)

    '                    ' 5) 台帳起票 + ロット減算 + 個体ステータス更新
    '                    For Each ln In lines
    '                        InsertLedgerIssue(header.IssueDate, issueId, header.RefType, header.RefId, ln, cn, tx)
    '                        UpdateLotOnHandMinus(ln, cn, tx)
    '                        If ln.LotUnitId.HasValue Then
    '                            UpdateLotUnitIssued(ln.LotUnitId.Value, issueId, cn, tx)
    '                        End If
    '                    Next

    '                    ' 6) posted を立てる
    '                    Using cmd = Db.CreateCmd(
    '"UPDATE issue_header
    ' SET posted=1, posted_at=NOW(), posted_by=@posted_by
    ' WHERE issue_id=@issue_id AND posted=0;", cn, tx)
    '                        Db.Param(cmd, "@posted_by", postedBy)
    '                        Db.Param(cmd, "@issue_id", issueId)
    '                        Dim n = cmd.ExecuteNonQuery()
    '                        If n = 0 Then Throw New Exception("確定に失敗しました（既に確定済みの可能性）。")
    '                    End Using

    '                    tx.Commit()
    '                Catch
    '                    tx.Rollback()
    '                    Throw
    '                End Try
    '            End Using
    '        End Using
    '    End Sub


    '    ' ==========
    '    ' REVERSE: 取消（逆仕訳：台帳＋、ロット加算、個体をON_HANDへ戻す例）
    '    ' ==========
    '    Public Sub Reverse(issueId As Long, reversedBy As String)
    '        If issueId <= 0 Then Throw New Exception("issueIdが不正です。")

    '        Using cn = Db.OpenConnection()
    '            Using tx = cn.BeginTransaction(IsolationLevel.ReadCommitted)
    '                Try
    '                    Dim header = GetHeaderForUpdate(issueId, cn, tx)
    '                    If header Is Nothing Then Throw New Exception("伝票が見つかりません。")
    '                    If Not header.Posted Then Throw New Exception("未確定のため取消できません。")

    '                    Dim lines = GetLines(issueId, cn, tx)
    '                    If lines.Count = 0 Then Throw New Exception("明細がありません。")

    '                    LockLotsAndUnits(lines, cn, tx)

    '                    ' 逆仕訳
    '                    For Each ln In lines
    '                        InsertLedgerReverse(header.IssueDate, issueId, header.RefType, header.RefId, ln, cn, tx)
    '                        UpdateLotOnHandPlus(ln, cn, tx)
    '                        If ln.LotUnitId.HasValue Then
    '                            UpdateLotUnitOnHand(ln.LotUnitId.Value, issueId, cn, tx)
    '                        End If
    '                    Next

    '                    ' posted解除（監査強度を上げたいなら posted=2 のように状態管理推奨）
    '                    Using cmd = Db.CreateCmd(
    '"UPDATE issue_header
    ' SET posted=0, posted_at=NULL, posted_by=NULL, memo=CONCAT(IFNULL(memo,''), ' [REVERSED BY ', @u, ' @', DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s'), ']')
    ' WHERE issue_id=@issue_id;", cn, tx)
    '                        Db.Param(cmd, "@u", reversedBy)
    '                        Db.Param(cmd, "@issue_id", issueId)
    '                        cmd.ExecuteNonQuery()
    '                    End Using

    '                    tx.Commit()
    '                Catch
    '                    tx.Rollback()
    '                    Throw
    '                End Try
    '            End Using
    '        End Using
    '    End Sub


    '    ' =========================
    '    ' 内部処理（DB）
    '    ' =========================

    '    Private Function GetHeaderForUpdate(issueId As Long, cn As MySqlConnection, tx As MySqlTransaction) As IssueHeader
    '        Dim sql =
    '"SELECT issue_id, issue_date, ref_type, ref_id, warehouse_id, memo, posted, posted_at, posted_by, created_by
    ' FROM issue_header
    ' WHERE issue_id=@issue_id
    ' FOR UPDATE;"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@issue_id", issueId)
    '            Using r = cmd.ExecuteReader()
    '                If Not r.Read() Then Return Nothing
    '                Dim h As New IssueHeader With {
    '                    .IssueId = r.GetInt64("issue_id"),
    '                    .IssueDate = r.GetDateTime("issue_date").Date,
    '                    .RefType = r.GetString("ref_type"),
    '                    .RefId = If(r.IsDBNull(r.GetOrdinal("ref_id")), CType(Nothing, Long?), r.GetInt64("ref_id")),
    '                    .WarehouseId = If(r.IsDBNull(r.GetOrdinal("warehouse_id")), CType(Nothing, Long?), r.GetInt64("warehouse_id")),
    '                    .Memo = If(r.IsDBNull(r.GetOrdinal("memo")), "", r.GetString("memo")),
    '                    .Posted = (r.GetInt32("posted") = 1),
    '                    .PostedAt = If(r.IsDBNull(r.GetOrdinal("posted_at")), CType(Nothing, DateTime?), r.GetDateTime("posted_at")),
    '                    .PostedBy = If(r.IsDBNull(r.GetOrdinal("posted_by")), "", r.GetString("posted_by")),
    '                    .CreatedBy = If(r.IsDBNull(r.GetOrdinal("created_by")), "", r.GetString("created_by"))
    '                }
    '                Return h
    '            End Using
    '        End Using
    '    End Function

    '    Private Function GetLines(issueId As Long, cn As MySqlConnection, tx As MySqlTransaction) As List(Of IssueLine)
    '        Dim list As New List(Of IssueLine)
    '        Dim sql =
    '"SELECT issue_line_id, issue_id, item_id, qty_pieces, lot_id, lot_unit_id, reason_code, note, alloc_key
    ' FROM issue_line
    ' WHERE issue_id=@issue_id
    ' ORDER BY issue_line_id;"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@issue_id", issueId)
    '            Using r = cmd.ExecuteReader()
    '                While r.Read()
    '                    list.Add(New IssueLine With {
    '                        .IssueLineId = r.GetInt64("issue_line_id"),
    '                        .IssueId = r.GetInt64("issue_id"),
    '                        .ItemId = r.GetInt64("item_id"),
    '                        .QtyPieces = r.GetDecimal("qty_pieces"),
    '                        .LotId = If(r.IsDBNull(r.GetOrdinal("lot_id")), CType(Nothing, Long?), r.GetInt64("lot_id")),
    '                        .LotUnitId = If(r.IsDBNull(r.GetOrdinal("lot_unit_id")), CType(Nothing, Long?), r.GetInt64("lot_unit_id")),
    '                        .ReasonCode = If(r.IsDBNull(r.GetOrdinal("reason_code")), "", r.GetString("reason_code")),
    '                        .Note = If(r.IsDBNull(r.GetOrdinal("note")), "", r.GetString("note")),
    '                        .AllocKey = If(r.IsDBNull(r.GetOrdinal("alloc_key")), "", r.GetString("alloc_key"))
    '                    })
    '                End While
    '            End Using
    '        End Using
    '        Return list
    '    End Function

    '    Private Sub LockLotsAndUnits(lines As List(Of IssueLine), cn As MySqlConnection, tx As MySqlTransaction)
    '        Dim lotIds = lines.Where(Function(x) x.LotId.HasValue).Select(Function(x) x.LotId.Value).Distinct().ToList()
    '        If lotIds.Count > 0 Then
    '            Dim inClause = String.Join(",", lotIds)
    '            Dim sql = $"SELECT lot_id FROM lot WHERE lot_id IN ({inClause}) FOR UPDATE;"
    '            Using cmd = Db.CreateCmd(sql, cn, tx)
    '                cmd.ExecuteNonQuery()
    '            End Using
    '        End If

    '        Dim unitIds = lines.Where(Function(x) x.LotUnitId.HasValue).Select(Function(x) x.LotUnitId.Value).Distinct().ToList()
    '        If unitIds.Count > 0 Then
    '            Dim inClause = String.Join(",", unitIds)
    '            Dim sql = $"SELECT lot_unit_id, status FROM lot_unit WHERE lot_unit_id IN ({inClause}) FOR UPDATE;"
    '            Using cmd = Db.CreateCmd(sql, cn, tx)
    '                cmd.ExecuteNonQuery()
    '            End Using
    '        End If
    '    End Sub

    '    Private Sub ValidateOnHand(lines As List(Of IssueLine), cn As MySqlConnection, tx As MySqlTransaction)
    '        ' ロット単位で合算して比較
    '        Dim grp = lines.Where(Function(x) x.LotId.HasValue).
    '            GroupBy(Function(x) x.LotId.Value).
    '            Select(Function(g) New With {.LotId = g.Key, .Qty = g.Sum(Function(x) x.QtyPieces)}).ToList()

    '        For Each g In grp
    '            Using cmd = Db.CreateCmd("SELECT qty_on_hand_pieces FROM lot WHERE lot_id=@lot_id;", cn, tx)
    '                Db.Param(cmd, "@lot_id", g.LotId)
    '                Dim onHand = Convert.ToDecimal(cmd.ExecuteScalar())
    '                If onHand < g.Qty Then
    '                    Throw New Exception($"在庫不足: lot_id={g.LotId} on_hand={onHand} issue={g.Qty}")
    '                End If
    '            End Using
    '        Next
    '    End Sub

    '    Private Sub InsertLedgerIssue(issueDate As Date, issueId As Long, refType As String, refId As Long?, ln As IssueLine, cn As MySqlConnection, tx As MySqlTransaction)
    '        ' ★あなたの inventory_ledger 列名に合わせて調整してOK
    '        Dim sql =
    '"INSERT INTO inventory_ledger(trx_date, ref_type, ref_id, item_id, lot_id, lot_unit_id, qty_delta, memo)
    ' VALUES(@trx_date, 'ISSUE', @issue_id, @item_id, @lot_id, @lot_unit_id, @qty_delta, @memo);"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@trx_date", issueDate)
    '            Db.Param(cmd, "@issue_id", issueId)
    '            Db.Param(cmd, "@item_id", ln.ItemId)
    '            Db.Param(cmd, "@lot_id", ln.LotId)
    '            Db.Param(cmd, "@lot_unit_id", ln.LotUnitId)
    '            Db.Param(cmd, "@qty_delta", -Math.Abs(ln.QtyPieces))
    '            Db.Param(cmd, "@memo", $"ISSUE ({refType}:{If(refId.HasValue, refId.Value.ToString(), "-")}) reason={ln.ReasonCode}")
    '            cmd.ExecuteNonQuery()
    '        End Using
    '    End Sub

    '    Private Sub InsertLedgerReverse(issueDate As Date, issueId As Long, refType As String, refId As Long?, ln As IssueLine, cn As MySqlConnection, tx As MySqlTransaction)
    '        Dim sql =
    '"INSERT INTO inventory_ledger(trx_date, ref_type, ref_id, item_id, lot_id, lot_unit_id, qty_delta, memo)
    ' VALUES(@trx_date, 'ISSUE_REV', @issue_id, @item_id, @lot_id, @lot_unit_id, @qty_delta, @memo);"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@trx_date", issueDate)
    '            Db.Param(cmd, "@issue_id", issueId)
    '            Db.Param(cmd, "@item_id", ln.ItemId)
    '            Db.Param(cmd, "@lot_id", ln.LotId)
    '            Db.Param(cmd, "@lot_unit_id", ln.LotUnitId)
    '            Db.Param(cmd, "@qty_delta", Math.Abs(ln.QtyPieces))
    '            Db.Param(cmd, "@memo", $"REVERSE ISSUE ({refType}:{If(refId.HasValue, refId.Value.ToString(), "-")})")
    '            cmd.ExecuteNonQuery()
    '        End Using
    '    End Sub

    '    Private Sub UpdateLotOnHandMinus(ln As IssueLine, cn As MySqlConnection, tx As MySqlTransaction)
    '        If Not ln.LotId.HasValue Then
    '            Throw New Exception("lot_id が未指定です（ロット管理品は必須）。")
    '        End If

    '        Dim sql =
    '"UPDATE lot
    ' SET qty_on_hand_pieces = qty_on_hand_pieces - @qty
    ' WHERE lot_id=@lot_id;"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@qty", Math.Abs(ln.QtyPieces))
    '            Db.Param(cmd, "@lot_id", ln.LotId.Value)
    '            cmd.ExecuteNonQuery()
    '        End Using
    '    End Sub

    '    Private Sub UpdateLotOnHandPlus(ln As IssueLine, cn As MySqlConnection, tx As MySqlTransaction)
    '        If Not ln.LotId.HasValue Then Throw New Exception("lot_id が未指定です。")

    '        Dim sql =
    '"UPDATE lot
    ' SET qty_on_hand_pieces = qty_on_hand_pieces + @qty
    ' WHERE lot_id=@lot_id;"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@qty", Math.Abs(ln.QtyPieces))
    '            Db.Param(cmd, "@lot_id", ln.LotId.Value)
    '            cmd.ExecuteNonQuery()
    '        End Using
    '    End Sub

    '    Private Sub UpdateLotUnitIssued(lotUnitId As Long, issueId As Long, cn As MySqlConnection, tx As MySqlTransaction)
    '        ' status が ALLOCATED/ON_HAND のみ許可（運用に合わせて）
    '        Dim sql =
    '"UPDATE lot_unit
    ' SET status='ISSUED', issued_at=NOW(), ref_type='ISSUE', ref_id=@issue_id
    ' WHERE lot_unit_id=@lot_unit_id AND status IN ('ALLOCATED','ON_HAND');"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@issue_id", issueId)
    '            Db.Param(cmd, "@lot_unit_id", lotUnitId)
    '            Dim n = cmd.ExecuteNonQuery()
    '            If n = 0 Then Throw New Exception($"lot_unit 更新不可: lot_unit_id={lotUnitId}（状態が許可されていない）")
    '        End Using
    '    End Sub

    '    Private Sub UpdateLotUnitOnHand(lotUnitId As Long, issueId As Long, cn As MySqlConnection, tx As MySqlTransaction)
    '        Dim sql =
    '"UPDATE lot_unit
    ' SET status='ON_HAND', ref_type=NULL, ref_id=NULL
    ' WHERE lot_unit_id=@lot_unit_id;"
    '        Using cmd = Db.CreateCmd(sql, cn, tx)
    '            Db.Param(cmd, "@lot_unit_id", lotUnitId)
    '            cmd.ExecuteNonQuery()
    '        End Using
    '    End Sub

    '    ' =========================
    '    ' 入力チェック
    '    ' =========================
    '    Private Sub ValidateLinesBasic(lines As List(Of IssueLine))
    '        For Each ln In lines
    '            If ln.ItemId <= 0 Then Throw New Exception("item_id が不正です。")
    '            If ln.QtyPieces <= 0D Then Throw New Exception("qty_pieces は 0 より大きい必要があります。")
    '            ' ロット必須運用（Sunstarの前提に合わせて）
    '            If Not ln.LotId.HasValue Then Throw New Exception("lot_id は必須です。")
    '        Next
    '    End Sub

End Class