Imports System.ComponentModel

Public Class ListViewEx
	Inherits ListView

#Region "Create and Destroy"

	Public Sub New()
		MyBase.New()

		Me.OwnerDraw = True
		Me.DoubleBuffered = True
	End Sub

#End Region

#Region "Init and Free"

	Private Sub Init()
		' [04-Feb-2026] Because Me.DesignMode is unreliable in nested widgets, must do this check to prevent a crash.
		If TheApp IsNot Nothing Then
			Me.UpdateTheme()
			AddHandler TheApp.Settings.PropertyChanged, AddressOf Me.AppSettings_PropertyChanged
		End If
	End Sub

	Private Sub Free()
		' [04-Feb-2026] Because Me.DesignMode is unreliable in nested widgets, must do this check to prevent a crash.
		If TheApp IsNot Nothing Then
			RemoveHandler TheApp.Settings.PropertyChanged, AddressOf Me.AppSettings_PropertyChanged
		End If
	End Sub

#End Region

#Region "Events"

	Protected Overrides Sub OnHandleCreated(ByVal e As System.EventArgs)
		MyBase.OnHandleCreated(e)

		Me.Init()
	End Sub

	Private Sub AppSettings_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
		If e.PropertyName = "AppThemeName" Then
			Me.UpdateTheme()
			Me.Invalidate()
		End If
	End Sub

	' NOTE: for View.Details (the only view mode this control is used in), the actual
	' cell-level painting happens in DrawSubItem below. DrawItem still fires once per
	' row first, but there is nothing useful to draw at that level for Details view,
	' so just let it fall through to the default (which, with OwnerDraw already
	' overriding everything else, is effectively a no-op for the row background -
	' DrawSubItem repaints the whole row's cells right after this anyway).
	Protected Overrides Sub OnDrawItem(ByVal e As DrawListViewItemEventArgs)
		If Me.View <> View.Details Then
			e.DrawDefault = True
		End If
	End Sub

	Protected Overrides Sub OnDrawColumnHeader(ByVal e As DrawListViewColumnHeaderEventArgs)
		Dim theme As ListViewTheme = Nothing
		If TheApp IsNot Nothing Then
			theme = TheApp.Settings.SelectedAppTheme.ListViewTheme
		End If

		If theme Is Nothing Then
			e.DrawDefault = True
			Exit Sub
		End If

		Using backBrush As New SolidBrush(theme.EnabledBackColor)
			e.Graphics.FillRectangle(backBrush, e.Bounds)
		End Using

		Dim textBounds As New Rectangle(e.Bounds.Left + 4, e.Bounds.Top, e.Bounds.Width - 8, e.Bounds.Height)
		TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, textBounds, theme.EnabledForeColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)

		Using borderPen As New Pen(theme.EnabledBorderColor)
			e.Graphics.DrawLine(borderPen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1)
			e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
		End Using
	End Sub

	Protected Overrides Sub OnDrawSubItem(ByVal e As DrawListViewSubItemEventArgs)
		Dim theme As ListViewTheme = Nothing
		If TheApp IsNot Nothing Then
			theme = TheApp.Settings.SelectedAppTheme.ListViewTheme
		End If

		If theme Is Nothing Then
			e.DrawDefault = True
			Exit Sub
		End If

		Dim isSelected As Boolean = e.Item.Selected
		Dim backColor As Color = theme.EnabledBackColor
		Dim foreColor As Color = theme.EnabledForeColor
		If isSelected Then
			backColor = theme.SelectedBackColor
			foreColor = theme.SelectedForeColor
		End If

		Using backBrush As New SolidBrush(backColor)
			e.Graphics.FillRectangle(backBrush, e.Bounds)
		End Using

		' NOTE: Only the first column (index 0) draws the row's icon, matching how a
		' normal (non-owner-drawn) details view ListView only shows the icon once,
		' next to the item's own text, not on every subitem/column.
		Dim textLeft As Integer = e.Bounds.Left + 4
		If e.ColumnIndex = 0 AndAlso Me.SmallImageList IsNot Nothing AndAlso e.Item.ImageIndex >= 0 AndAlso e.Item.ImageIndex < Me.SmallImageList.Images.Count Then
			Dim imageTop As Integer = e.Bounds.Top + CInt((e.Bounds.Height - Me.SmallImageList.ImageSize.Height) / 2)
			e.Graphics.DrawImage(Me.SmallImageList.Images(e.Item.ImageIndex), textLeft, imageTop, Me.SmallImageList.ImageSize.Width, Me.SmallImageList.ImageSize.Height)
			textLeft += Me.SmallImageList.ImageSize.Width + 4
		End If

		Dim textBounds As New Rectangle(textLeft, e.Bounds.Top, Math.Max(0, e.Bounds.Right - textLeft - 2), e.Bounds.Height)
		TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, textBounds, foreColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)

		If isSelected Then
			Using borderPen As New Pen(theme.SelectedBorderColor)
				e.Graphics.DrawRectangle(borderPen, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1)
			End Using
		End If
	End Sub

#End Region

#Region "Private Methods"

	Private Sub UpdateTheme()
		Dim theme As ListViewTheme = Nothing
		If TheApp IsNot Nothing Then
			theme = TheApp.Settings.SelectedAppTheme.ListViewTheme
		End If

		If theme IsNot Nothing Then
			MyBase.BackColor = theme.EnabledBackColor
			Me.ForeColor = theme.EnabledForeColor
		Else
			MyBase.BackColor = Control.DefaultBackColor
			Me.ForeColor = Control.DefaultForeColor
		End If
	End Sub

#End Region

End Class
