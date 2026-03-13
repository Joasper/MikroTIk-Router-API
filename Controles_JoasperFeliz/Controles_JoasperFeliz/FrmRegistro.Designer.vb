<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmRegistro
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        txtNombre = New TextBox()
        txtContrasena = New TextBox()
        txtCorreo = New TextBox()
        chkVerClave = New CheckBox()
        grpGenero = New GroupBox()
        txtOtroGenero = New TextBox()
        rbOtro = New RadioButton()
        rbMasculino = New RadioButton()
        rbFemenino = New RadioButton()
        gbNotificaciones = New GroupBox()
        chkPush = New CheckBox()
        chkSms = New CheckBox()
        chkEmail = New CheckBox()
        grbRol = New GroupBox()
        lblValorRol = New Label()
        lblIndiceRol = New Label()
        cboRol = New ComboBox()
        Label4 = New Label()
        grbInteres = New GroupBox()
        lblInteresSeleccionado = New Label()
        lstIntereses = New ListBox()
        Label5 = New Label()
        Label6 = New Label()
        Label7 = New Label()
        chkModoAvanzado = New CheckBox()
        pnlAvanzado = New Panel()
        TextBox1 = New TextBox()
        Label8 = New Label()
        btnLimpiar = New Button()
        btnCancelar = New Button()
        btnGuardar = New Button()
        grpGenero.SuspendLayout()
        gbNotificaciones.SuspendLayout()
        grbRol.SuspendLayout()
        grbInteres.SuspendLayout()
        pnlAvanzado.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(44, 49)
        Label1.Name = "Label1"
        Label1.Size = New Size(78, 25)
        Label1.TabIndex = 0
        Label1.Text = "&Nombre"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(44, 121)
        Label2.Name = "Label2"
        Label2.Size = New Size(66, 25)
        Label2.TabIndex = 1
        Label2.Text = "&Correo"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(44, 199)
        Label3.Name = "Label3"
        Label3.Size = New Size(101, 25)
        Label3.TabIndex = 2
        Label3.Text = "&Contraseña"
        ' 
        ' txtNombre
        ' 
        txtNombre.Location = New Point(172, 54)
        txtNombre.Name = "txtNombre"
        txtNombre.Size = New Size(150, 31)
        txtNombre.TabIndex = 3
        ' 
        ' txtContrasena
        ' 
        txtContrasena.Location = New Point(172, 196)
        txtContrasena.Name = "txtContrasena"
        txtContrasena.Size = New Size(150, 31)
        txtContrasena.TabIndex = 4
        txtContrasena.UseSystemPasswordChar = True
        ' 
        ' txtCorreo
        ' 
        txtCorreo.Location = New Point(172, 121)
        txtCorreo.Name = "txtCorreo"
        txtCorreo.Size = New Size(150, 31)
        txtCorreo.TabIndex = 5
        ' 
        ' chkVerClave
        ' 
        chkVerClave.AutoSize = True
        chkVerClave.Location = New Point(342, 199)
        chkVerClave.Name = "chkVerClave"
        chkVerClave.Size = New Size(63, 29)
        chkVerClave.TabIndex = 6
        chkVerClave.Text = "&Ver"
        chkVerClave.UseVisualStyleBackColor = True
        ' 
        ' grpGenero
        ' 
        grpGenero.Controls.Add(txtOtroGenero)
        grpGenero.Controls.Add(rbOtro)
        grpGenero.Controls.Add(rbMasculino)
        grpGenero.Controls.Add(rbFemenino)
        grpGenero.Location = New Point(44, 275)
        grpGenero.Name = "grpGenero"
        grpGenero.Size = New Size(361, 258)
        grpGenero.TabIndex = 7
        grpGenero.TabStop = False
        grpGenero.Text = "Género"
        ' 
        ' txtOtroGenero
        ' 
        txtOtroGenero.Enabled = False
        txtOtroGenero.Location = New Point(128, 166)
        txtOtroGenero.Name = "txtOtroGenero"
        txtOtroGenero.Size = New Size(150, 31)
        txtOtroGenero.TabIndex = 8
        ' 
        ' rbOtro
        ' 
        rbOtro.AutoSize = True
        rbOtro.Location = New Point(14, 166)
        rbOtro.Name = "rbOtro"
        rbOtro.Size = New Size(74, 29)
        rbOtro.TabIndex = 2
        rbOtro.TabStop = True
        rbOtro.Text = "Otro"
        rbOtro.UseVisualStyleBackColor = True
        ' 
        ' rbMasculino
        ' 
        rbMasculino.AutoSize = True
        rbMasculino.Location = New Point(14, 103)
        rbMasculino.Name = "rbMasculino"
        rbMasculino.Size = New Size(117, 29)
        rbMasculino.TabIndex = 1
        rbMasculino.TabStop = True
        rbMasculino.Text = "Masculino"
        rbMasculino.UseVisualStyleBackColor = True
        ' 
        ' rbFemenino
        ' 
        rbFemenino.AutoSize = True
        rbFemenino.Location = New Point(14, 41)
        rbFemenino.Name = "rbFemenino"
        rbFemenino.Size = New Size(115, 29)
        rbFemenino.TabIndex = 0
        rbFemenino.TabStop = True
        rbFemenino.Text = "Femenino"
        rbFemenino.UseVisualStyleBackColor = True
        ' 
        ' gbNotificaciones
        ' 
        gbNotificaciones.Controls.Add(chkPush)
        gbNotificaciones.Controls.Add(chkSms)
        gbNotificaciones.Controls.Add(chkEmail)
        gbNotificaciones.Location = New Point(475, 275)
        gbNotificaciones.Name = "gbNotificaciones"
        gbNotificaciones.Size = New Size(361, 258)
        gbNotificaciones.TabIndex = 9
        gbNotificaciones.TabStop = False
        gbNotificaciones.Text = "Notificaciones"
        ' 
        ' chkPush
        ' 
        chkPush.AutoSize = True
        chkPush.Location = New Point(31, 168)
        chkPush.Name = "chkPush"
        chkPush.Size = New Size(76, 29)
        chkPush.TabIndex = 2
        chkPush.Text = "Push"
        chkPush.UseVisualStyleBackColor = True
        ' 
        ' chkSms
        ' 
        chkSms.AutoSize = True
        chkSms.Location = New Point(31, 104)
        chkSms.Name = "chkSms"
        chkSms.Size = New Size(74, 29)
        chkSms.TabIndex = 1
        chkSms.Text = "SMS"
        chkSms.UseVisualStyleBackColor = True
        ' 
        ' chkEmail
        ' 
        chkEmail.AutoSize = True
        chkEmail.Location = New Point(31, 42)
        chkEmail.Name = "chkEmail"
        chkEmail.Size = New Size(80, 29)
        chkEmail.TabIndex = 0
        chkEmail.Text = "Email"
        chkEmail.UseVisualStyleBackColor = True
        ' 
        ' grbRol
        ' 
        grbRol.Controls.Add(lblValorRol)
        grbRol.Controls.Add(lblIndiceRol)
        grbRol.Controls.Add(cboRol)
        grbRol.Controls.Add(Label4)
        grbRol.Location = New Point(475, 12)
        grbRol.Name = "grbRol"
        grbRol.Size = New Size(361, 258)
        grbRol.TabIndex = 10
        grbRol.TabStop = False
        grbRol.Text = "Roles"
        ' 
        ' lblValorRol
        ' 
        lblValorRol.AutoSize = True
        lblValorRol.Location = New Point(223, 115)
        lblValorRol.Name = "lblValorRol"
        lblValorRol.Size = New Size(68, 25)
        lblValorRol.TabIndex = 14
        lblValorRol.Text = "Valor: -"
        ' 
        ' lblIndiceRol
        ' 
        lblIndiceRol.AutoSize = True
        lblIndiceRol.Location = New Point(104, 109)
        lblIndiceRol.Name = "lblIndiceRol"
        lblIndiceRol.Size = New Size(75, 25)
        lblIndiceRol.TabIndex = 13
        lblIndiceRol.Text = "Índice: -"
        ' 
        ' cboRol
        ' 
        cboRol.FormattingEnabled = True
        cboRol.Location = New Point(104, 42)
        cboRol.Name = "cboRol"
        cboRol.Size = New Size(228, 33)
        cboRol.TabIndex = 12
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(31, 45)
        Label4.Name = "Label4"
        Label4.Size = New Size(54, 25)
        Label4.TabIndex = 11
        Label4.Text = "&Roles"
        ' 
        ' grbInteres
        ' 
        grbInteres.Controls.Add(lblInteresSeleccionado)
        grbInteres.Controls.Add(lstIntereses)
        grbInteres.Controls.Add(Label5)
        grbInteres.Controls.Add(Label6)
        grbInteres.Controls.Add(Label7)
        grbInteres.Location = New Point(853, 12)
        grbInteres.Name = "grbInteres"
        grbInteres.Size = New Size(361, 258)
        grbInteres.TabIndex = 15
        grbInteres.TabStop = False
        grbInteres.Text = "Intereses"
        ' 
        ' lblInteresSeleccionado
        ' 
        lblInteresSeleccionado.AutoSize = True
        lblInteresSeleccionado.Location = New Point(31, 210)
        lblInteresSeleccionado.Name = "lblInteresSeleccionado"
        lblInteresSeleccionado.Size = New Size(194, 25)
        lblInteresSeleccionado.TabIndex = 15
        lblInteresSeleccionado.Text = "Seleccionado: Ninguno"
        ' 
        ' lstIntereses
        ' 
        lstIntereses.FormattingEnabled = True
        lstIntereses.ItemHeight = 25
        lstIntereses.Items.AddRange(New Object() {"IA", "Programación", "Bases de datos", "Redes"})
        lstIntereses.Location = New Point(137, 54)
        lstIntereses.Name = "lstIntereses"
        lstIntereses.Size = New Size(180, 129)
        lstIntereses.TabIndex = 15
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(223, 115)
        Label5.Name = "Label5"
        Label5.Size = New Size(0, 25)
        Label5.TabIndex = 14
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(104, 109)
        Label6.Name = "Label6"
        Label6.Size = New Size(0, 25)
        Label6.TabIndex = 13
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(31, 45)
        Label7.Name = "Label7"
        Label7.Size = New Size(86, 25)
        Label7.TabIndex = 11
        Label7.Text = "&Intereses:"
        ' 
        ' chkModoAvanzado
        ' 
        chkModoAvanzado.AutoSize = True
        chkModoAvanzado.Location = New Point(866, 332)
        chkModoAvanzado.Name = "chkModoAvanzado"
        chkModoAvanzado.Size = New Size(168, 29)
        chkModoAvanzado.TabIndex = 16
        chkModoAvanzado.Text = "&Modo avanzado"
        chkModoAvanzado.UseVisualStyleBackColor = True
        ' 
        ' pnlAvanzado
        ' 
        pnlAvanzado.Controls.Add(TextBox1)
        pnlAvanzado.Controls.Add(Label8)
        pnlAvanzado.Location = New Point(853, 375)
        pnlAvanzado.Name = "pnlAvanzado"
        pnlAvanzado.Size = New Size(300, 158)
        pnlAvanzado.TabIndex = 17
        pnlAvanzado.Visible = False
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(31, 68)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(150, 31)
        TextBox1.TabIndex = 1
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(21, 36)
        Label8.Name = "Label8"
        Label8.Size = New Size(155, 25)
        Label8.TabIndex = 0
        Label8.Text = "Notas adicionales:"
        ' 
        ' btnLimpiar
        ' 
        btnLimpiar.Location = New Point(772, 554)
        btnLimpiar.Name = "btnLimpiar"
        btnLimpiar.Size = New Size(112, 34)
        btnLimpiar.TabIndex = 18
        btnLimpiar.Text = "&Limpiar"
        btnLimpiar.UseVisualStyleBackColor = True
        ' 
        ' btnCancelar
        ' 
        btnCancelar.Location = New Point(922, 554)
        btnCancelar.Name = "btnCancelar"
        btnCancelar.Size = New Size(112, 34)
        btnCancelar.TabIndex = 19
        btnCancelar.Text = "&Salir"
        btnCancelar.UseVisualStyleBackColor = True
        ' 
        ' btnGuardar
        ' 
        btnGuardar.Location = New Point(1058, 554)
        btnGuardar.Name = "btnGuardar"
        btnGuardar.Size = New Size(112, 34)
        btnGuardar.TabIndex = 20
        btnGuardar.Text = "&Guardar"
        btnGuardar.UseVisualStyleBackColor = True
        ' 
        ' FrmRegistro
        ' 
        AcceptButton = btnGuardar
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancelar
        ClientSize = New Size(1243, 612)
        Controls.Add(btnGuardar)
        Controls.Add(btnCancelar)
        Controls.Add(btnLimpiar)
        Controls.Add(pnlAvanzado)
        Controls.Add(chkModoAvanzado)
        Controls.Add(grbInteres)
        Controls.Add(grbRol)
        Controls.Add(gbNotificaciones)
        Controls.Add(grpGenero)
        Controls.Add(chkVerClave)
        Controls.Add(txtCorreo)
        Controls.Add(txtContrasena)
        Controls.Add(txtNombre)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Name = "FrmRegistro"
        Text = "Registro de Usuarios"
        grpGenero.ResumeLayout(False)
        grpGenero.PerformLayout()
        gbNotificaciones.ResumeLayout(False)
        gbNotificaciones.PerformLayout()
        grbRol.ResumeLayout(False)
        grbRol.PerformLayout()
        grbInteres.ResumeLayout(False)
        grbInteres.PerformLayout()
        pnlAvanzado.ResumeLayout(False)
        pnlAvanzado.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents txtNombre As TextBox
    Friend WithEvents txtContrasena As TextBox
    Friend WithEvents txtCorreo As TextBox
    Friend WithEvents chkVerClave As CheckBox
    Friend WithEvents grpGenero As GroupBox
    Friend WithEvents rbOtro As RadioButton
    Friend WithEvents rbMasculino As RadioButton
    Friend WithEvents rbFemenino As RadioButton
    Friend WithEvents txtOtroGenero As TextBox
    Friend WithEvents gbNotificaciones As GroupBox
    Friend WithEvents chkPush As CheckBox
    Friend WithEvents chkSms As CheckBox
    Friend WithEvents chkEmail As CheckBox
    Friend WithEvents grbRol As GroupBox
    Friend WithEvents cboRol As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents lblValorRol As Label
    Friend WithEvents lblIndiceRol As Label
    Friend WithEvents grbInteres As GroupBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents lblInteresSeleccionado As Label
    Friend WithEvents lstIntereses As ListBox
    Friend WithEvents chkModoAvanzado As CheckBox
    Friend WithEvents pnlAvanzado As Panel
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents btnLimpiar As Button
    Friend WithEvents btnCancelar As Button
    Friend WithEvents btnGuardar As Button

End Class
