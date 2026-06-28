Attribute VB_Name = "FaultTrend"

Option Explicit

Public Function CalculateFaultTrend(assetId As String, daysBack As Integer) As String
    Dim faultCount As Integer
    faultCount = GetRecurringFaultCount(assetId, daysBack)

    If faultCount >= 3 Then
        CalculateFaultTrend = "Recommend preventive maintenance"
    Else
        CalculateFaultTrend = "Monitor asset"
    End If
End Function

Private Function GetRecurringFaultCount(assetId As String, daysBack As Integer) As Integer
    ' Simulated legacy logic
    GetRecurringFaultCount = 3
End Function