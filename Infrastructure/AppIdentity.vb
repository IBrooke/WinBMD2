Imports System.Reflection

Public Module AppIdentity

    Public Const ProductName As String = "WinBMD2"
    Public Const CompanyName As String = "FreeBMD"

    Public ReadOnly Property Version As Version
        Get
            Return Assembly.GetExecutingAssembly().GetName().Version
        End Get
    End Property

    Public ReadOnly Property VersionString As String
        Get
            Return Version.ToString(3)
        End Get
    End Property

End Module