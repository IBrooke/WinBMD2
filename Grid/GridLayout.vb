Imports System.Linq

Public Module GridLayout

    Private NotInheritable Class LayoutDefinition

        Public Property BatchType As String = ""

        Public Property FromYear As Integer
        Public Property FromQuarter As Integer

        Public Property ToYear As Integer
        Public Property ToQuarter As Integer

        Public Property Fields As GridField() = Array.Empty(Of GridField)()

    End Class

    Private ReadOnly Layouts As New List(Of LayoutDefinition) From {
        New LayoutDefinition With {
            .BatchType = "D",
            .FromYear = 1837,
            .FromQuarter = 1,
            .ToYear = 1969,
            .ToQuarter = 1,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.AaD,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "D",
            .FromYear = 1969,
            .FromQuarter = 2,
            .ToYear = 1983,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.DoB,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "D",
            .FromYear = 1984,
            .FromQuarter = 1,
            .ToYear = 1992,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.DoB,
                GridField.District,
                GridField.Reg,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "D",
            .FromYear = 1993,
            .FromQuarter = 1,
            .ToYear = 9999,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.DoB,
                GridField.District,
                GridField.DistNum,
                GridField.RegNum,
                GridField.Entry,
                GridField.DoR
            }
        },
        New LayoutDefinition With {
            .BatchType = "M",
            .FromYear = 1837,
            .FromQuarter = 1,
            .ToYear = 1911,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "M",
            .FromYear = 1912,
            .FromQuarter = 1,
            .ToYear = 1983,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Spouse,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "M",
            .FromYear = 1984,
            .FromQuarter = 1,
            .ToYear = 1992,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Spouse,
                GridField.District,
                GridField.Reg,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "M",
            .FromYear = 1993,
            .FromQuarter = 1,
            .ToYear = 1993,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Spouse,
                GridField.District,
                GridField.DoR,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "M",
            .FromYear = 1994,
            .FromQuarter = 1,
            .ToYear = 9999,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Spouse,
                GridField.District,
                GridField.Volume,
                GridField.Month,
                GridField.Page,
                GridField.Entry,
                GridField.Source
            }
        },
        New LayoutDefinition With {
            .BatchType = "B",
            .FromYear = 1837,
            .FromQuarter = 1,
            .ToYear = 1911,
            .ToQuarter = 2,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "B",
            .FromYear = 1911,
            .FromQuarter = 3,
            .ToYear = 1983,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Mother,
                GridField.District,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "B",
            .FromYear = 1984,
            .FromQuarter = 1,
            .ToYear = 1992,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Mother,
                GridField.District,
                GridField.Reg,
                GridField.Volume,
                GridField.Page
            }
        },
        New LayoutDefinition With {
            .BatchType = "B",
            .FromYear = 1993,
            .FromQuarter = 1,
            .ToYear = 9999,
            .ToQuarter = 4,
            .Fields = New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.Mother,
                GridField.District,
                GridField.DistNum,
                GridField.RegNum,
                GridField.Entry,
                GridField.DoR
            }
        }
    }

    Public Function GetVisibleFields() As GridField()

        If ProjectValues.Year <= 0 OrElse
           String.IsNullOrWhiteSpace(ProjectValues.BatchType) Then

            Return New GridField() {
                GridField.Surname,
                GridField.Forename,
                GridField.District,
                GridField.Volume,
                GridField.Page,
                GridField.Directive,
                GridField.Verified
            }

        End If

        Dim layout As LayoutDefinition =
            Layouts.FirstOrDefault(
                Function(item)
                    Return item.BatchType = ProjectValues.BatchType AndAlso
                           IsOnOrAfter(
                               ProjectValues.Year,
                               ProjectValues.Quarter,
                               item.FromYear,
                               item.FromQuarter) AndAlso
                           IsOnOrBefore(
                               ProjectValues.Year,
                               ProjectValues.Quarter,
                               item.ToYear,
                               item.ToQuarter)
                End Function)

        If layout Is Nothing Then
            Throw New InvalidOperationException(
                "Unsupported layout: " &
                "BatchType=" & ProjectValues.BatchType &
                ", Year=" & ProjectValues.Year.ToString() &
                ", Quarter=" & ProjectValues.Quarter.ToString())
        End If

        Return layout.Fields.
            Concat(
                New GridField() {
                    GridField.Directive,
                    GridField.Verified
                }).
            ToArray()

    End Function

    Private Function IsOnOrAfter(
        year As Integer,
        quarter As Integer,
        fromYear As Integer,
        fromQuarter As Integer) As Boolean

        Return year > fromYear OrElse
               (year = fromYear AndAlso
                (quarter = 0 OrElse quarter >= fromQuarter))

    End Function

    Private Function IsOnOrBefore(
        year As Integer,
        quarter As Integer,
        toYear As Integer,
        toQuarter As Integer) As Boolean

        Return year < toYear OrElse
               (year = toYear AndAlso
                (quarter = 0 OrElse quarter <= toQuarter))

    End Function

End Module