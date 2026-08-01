Imports System.Collections.Generic
'------------------------------------------------------------------------------
' FieldMetaData
'
' Holds the static definition of every GridField used by WinBMD2.
'
' For each field, this module defines the information needed by the
' transcription grid to display and work with that field, including:
'
'     • Column heading.
'     • Text alignment.
'     • Preferred and minimum column widths.
'     • Whether the field supports a picklist.
'     • Whether the field is a Volume-style field.
'     • Whether the field represents transcription data.
'
' The metadata is independent of the current batch. GridLayout determines
' which GridFields are visible for the current Birth, Marriage or Death
' layout, while FieldMetaData describes how each of those fields should
' appear and behave.
'
' Additional information, such as validation routines, will be added here
' as the application develops.
'------------------------------------------------------------------------------
Public Module FieldMetaData

    Private ReadOnly _meta As New Dictionary(Of GridField, FieldMeta) From {
        {
            GridField.Surname,
            New FieldMeta With {
                .Header = "Surname",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 150,
                .MinWidth = 90
            }
        },
        {
            GridField.Forename,
            New FieldMeta With {
                .Header = "Forenames",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 160,
                .MinWidth = 100,
                .UsesPicklist = True
            }
        },
        {
            GridField.District,
            New FieldMeta With {
                .Header = "District",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 130,
                .MinWidth = 90,
                .UsesPicklist = True
            }
        },
        {
            GridField.Volume,
            New FieldMeta With {
                .Header = "Vol",
                .Align = CellTextAlign.Right,
                .PreferredWidth = 55,
                .MinWidth = 45,
                .IsVolumeField = True
            }
        },
        {
            GridField.DistNum,
            New FieldMeta With {
                .Header = "No.",
                .Align = CellTextAlign.Right,
                .PreferredWidth = 55,
                .MinWidth = 45,
                .IsVolumeField = True
            }
        },
        {
            GridField.Page,
            New FieldMeta With {
                .Header = "Page",
                .Align = CellTextAlign.Right,
                .PreferredWidth = 60,
                .MinWidth = 50
            }
        },
        {
            GridField.AaD,
            New FieldMeta With {
                .Header = "AaD",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 55,
                .MinWidth = 45
            }
        },
        {
            GridField.Mother,
            New FieldMeta With {
                .Header = "Mother",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 140,
                .MinWidth = 90
            }
        },
        {
            GridField.Spouse,
            New FieldMeta With {
                .Header = "Spouse",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 140,
                .MinWidth = 90
            }
        },
        {
            GridField.DoR,
            New FieldMeta With {
                .Header = "DoR",
                .Align = CellTextAlign.Right,
                .PreferredWidth = 65,
                .MinWidth = 50
            }
        },
        {
            GridField.Reg,
            New FieldMeta With {
                .Header = "Reg",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 60,
                .MinWidth = 45
            }
        },
        {
            GridField.RegNum,
            New FieldMeta With {
                .Header = "Reg No",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 75,
                .MinWidth = 55
            }
        },
        {
            GridField.Entry,
            New FieldMeta With {
                .Header = "Entry",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 65,
                .MinWidth = 50
            }
        },
        {
            GridField.Month,
            New FieldMeta With {
                .Header = "Month",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 70,
                .MinWidth = 55
            }
        },
        {
            GridField.Source,
            New FieldMeta With {
                .Header = "Source",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 75,
                .MinWidth = 55
            }
        },
        {
            GridField.DoB,
            New FieldMeta With {
                .Header = "DoB",
                .Align = CellTextAlign.Left,
                .PreferredWidth = 85,
                .MinWidth = 65
            }
        },
        {
            GridField.Directive,
            New FieldMeta With {
                .Header = "Directives",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 100,
                .MinWidth = 90,
                .IsDataColumn = False
            }
        },
        {
            GridField.Verified,
            New FieldMeta With {
                .Header = "Verified",
                .Align = CellTextAlign.Center,
                .PreferredWidth = 75,
                .MinWidth = 70,
                .IsDataColumn = False
            }
        }
    }

    Public ReadOnly Property Meta As IReadOnlyDictionary(Of GridField, FieldMeta)
        Get
            Return _meta
        End Get
    End Property

    Public Function GetLastDataColumn() As Integer

        Dim fields() As GridField = GridLayout.GetVisibleFields()

        For index As Integer = fields.Length - 1 To 0 Step -1

            Dim fieldInformation As FieldMeta = _meta(fields(index))

            If fieldInformation.IsDataColumn Then
                Return index
            End If

        Next

        Return 0

    End Function

End Module