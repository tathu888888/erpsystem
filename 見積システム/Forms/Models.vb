Public Class IssueHeader
    Public Property IssueId As Long
    Public Property IssueDate As Date
    Public Property RefType As String
    Public Property RefId As Long?
    Public Property WarehouseId As Long?
    Public Property Memo As String
    Public Property Posted As Boolean
    Public Property PostedAt As DateTime?
    Public Property PostedBy As String
    Public Property CreatedBy As String
End Class

Public Class IssueLine
    Public Property IssueLineId As Long
    Public Property IssueId As Long
    Public Property ItemId As Long
    Public Property QtyPieces As Decimal
    Public Property LotId As Long?
    Public Property LotUnitId As Long?
    Public Property ReasonCode As String
    Public Property Note As String
    Public Property AllocKey As String
End Class