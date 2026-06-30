Attribute VB_Name = "FaultTrend"

' ============================================================
' FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY
' Simulates VB6-style fault trend analysis and recommendation logic.
' ============================================================

Option Explicit

Private Const DEFAULT_DAYS_BACK As Integer = 30
Private Const RECURRING_FAULT_THRESHOLD As Integer = 3
Private Const HIGH_SEVERITY_THRESHOLD As Integer = 2

Public Function CalculateFaultTrend(assetId As String, daysBack As Integer) As String
    ' Analyzes fault history for a given asset and returns a recommendation.

    Dim faultCount As Integer
    Dim highSeverityCount As Integer

    If daysBack <= 0 Then
        daysBack = DEFAULT_DAYS_BACK
    End If

    faultCount = GetRecurringFaultCount(assetId, daysBack)
    highSeverityCount = GetHighSeverityFaultCount(assetId, daysBack)

    CalculateFaultTrend = GetRecommendationForFault("GENERAL", faultCount, highSeverityCount)
End Function


Public Function GetRecurringFaultCount(assetId As String, daysBack As Integer) As Integer
    ' Counts how many times the same fault code recurs within the period.
    ' In production, this queries the FaultEvents database table.
    GetRecurringFaultCount = 3
End Function


Public Function GetHighSeverityFaultCount(assetId As String, daysBack As Integer) As Integer
    ' Counts faults classified as HIGH severity within the period.
    GetHighSeverityFaultCount = 2
End Function


Public Function GetRecommendationForFault(faultCode As String, faultCount As Integer, highSeverityCount As Integer) As String
    ' Business rule engine for fault recommendations.
    '
    ' Rules:
    ' 1. If same fault occurs 3+ times -> recommend preventive maintenance
    ' 2. If HIGH severity faults >= 2 -> recommend urgent inspection
    ' 3. Otherwise -> recommend monitoring

    If highSeverityCount >= HIGH_SEVERITY_THRESHOLD Then
        GetRecommendationForFault = "URGENT: Schedule immediate inspection. High-severity faults detected (" & CStr(highSeverityCount) & " occurrences)."
    ElseIf faultCount >= RECURRING_FAULT_THRESHOLD Then
        GetRecommendationForFault = "PREVENTIVE: Schedule preventive maintenance. Recurring fault pattern detected (" & CStr(faultCount) & " occurrences)."
    Else
        GetRecommendationForFault = "MONITOR: Continue monitoring. No actionable fault pattern detected."
    End If
End Function
