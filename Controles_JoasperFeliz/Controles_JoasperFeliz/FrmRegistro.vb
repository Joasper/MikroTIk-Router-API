Public Class FrmRegistro

    Private Sub FrmRegistro_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim roles As New Dictionary(Of String, String) From {
            {"Admin", "Administrador"},
            {"User", "Usuario"},
            {"Guest", "Invitado"}
        }

        cboRol.DataSource = New BindingSource(roles, Nothing)
        cboRol.DisplayMember = "Value"
        cboRol.ValueMember = "Key"
        cboRol.SelectedIndex = -1

        txtOtroGenero.Enabled = False
        btnGuardar.Enabled = False
    End Sub

    Private Sub ValidarCampos()
        Dim nombreValido As Boolean = Not String.IsNullOrWhiteSpace(txtNombre.Text)
        Dim correoValido As Boolean = Not String.IsNullOrWhiteSpace(txtCorreo.Text)
        Dim claveValida As Boolean = txtContrasena.Text.Length >= 6
        Dim rolValido As Boolean = cboRol.SelectedIndex <> -1

        btnGuardar.Enabled = (nombreValido AndAlso correoValido AndAlso claveValida AndAlso rolValido)
    End Sub

    Private Sub txtNombre_TextChanged(sender As Object, e As EventArgs) Handles txtNombre.TextChanged, txtCorreo.TextChanged, txtContrasena.TextChanged
        ValidarCampos()
    End Sub

    Private Sub chkVerClave_CheckedChanged(sender As Object, e As EventArgs) Handles chkVerClave.CheckedChanged
        txtContrasena.UseSystemPasswordChar = Not chkVerClave.Checked
    End Sub
    Private Sub rdbOtro_CheckedChanged(sender As Object, e As EventArgs) Handles rbOtro.CheckedChanged
        txtOtroGenero.Enabled = rbOtro.Checked
        If rbOtro.Checked Then
            txtOtroGenero.Focus()
        Else
            txtOtroGenero.Clear()
        End If
    End Sub

    Private Sub chkModoAvanzado_CheckedChanged(sender As Object, e As EventArgs) Handles chkModoAvanzado.CheckedChanged
        pnlAvanzado.Visible = chkModoAvanzado.Checked
    End Sub

    Private Sub cboRol_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboRol.SelectedIndexChanged
        If cboRol.SelectedIndex <> -1 Then
            lblIndiceRol.Text = "Índice: " & cboRol.SelectedIndex.ToString()
            lblValorRol.Text = "Valor: " & cboRol.SelectedValue.ToString()
        Else
            lblIndiceRol.Text = "Índice: -"
            lblValorRol.Text = "Valor: -"
        End If
        ValidarCampos()
    End Sub

    Private Sub lstIntereses_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstIntereses.SelectedIndexChanged
        If lstIntereses.SelectedIndex <> -1 Then
            lblInteresSeleccionado.Text = "Seleccionado: " & lstIntereses.SelectedItem.ToString()
        End If
    End Sub


    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        MessageBox.Show("¡Registro guardado exitosamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
        btnLimpiar.PerformClick()
    End Sub

    Private Sub btnLimpiar_Click(sender As Object, e As EventArgs) Handles btnLimpiar.Click
        txtNombre.Clear()
        txtCorreo.Clear()
        txtContrasena.Clear()
        txtOtroGenero.Clear()

        chkVerClave.Checked = False
        rbFemenino.Checked = False
        rbMasculino.Checked = False
        rbOtro.Checked = False

        chkEmail.Checked = False
        chkSms.Checked = False
        chkPush.Checked = False
        chkModoAvanzado.Checked = False

        cboRol.SelectedIndex = -1
        lstIntereses.ClearSelected()

        txtNombre.Focus()
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        Me.Close()
    End Sub

End Class
