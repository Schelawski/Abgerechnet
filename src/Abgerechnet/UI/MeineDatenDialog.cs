using Abgerechnet.Core.Storage;

namespace Abgerechnet.UI;

/// <summary>
/// Dialog "Meine Daten": edits the sender's data and the invoice settings and saves them to
/// <c>abgerechnet.json</c>. Cancel leaves everything unchanged.
/// </summary>
internal sealed class MeineDatenDialog : Form
{
    private readonly DataFolder _folder;
    private readonly MeineDatenPanel _panel = new();

    public MeineDatenDialog(DataFolder folder)
    {
        _folder = folder;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = UiText.MeineDatenTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(640, 620);
        MinimumSize = new Size(560, 480);
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;

        var saveButton = UiStyle.CreateButton(UiText.Save);
        UiStyle.MakePrimary(saveButton);
        saveButton.Click += (_, _) => Save();
        var cancelButton = UiStyle.CreateButton(UiText.Cancel);
        cancelButton.DialogResult = DialogResult.Cancel;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(8),
        };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        var panelHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 0) };
        panelHost.Controls.Add(_panel);

        Controls.Add(panelHost);
        Controls.Add(buttons);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        ResumeLayout(false);
        PerformLayout();

        UiStyle.EnableHelpKey(this, Core.Help.HelpTopics.MeineDaten);
        _panel.LoadFrom(folder.Einstellungen, Core.Vorlagen.MitgelieferteVorlagen.Verfuegbare(folder));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStyle.FitToScreen(this);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _panel.FocusFirstField();
    }

    private void Save()
    {
        if (_panel.CheckInput() is { } problem)
        {
            MessageBox.Show(this, problem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_panel.SteuernummerFehlt
            && MessageBox.Show(this, UiText.SteuernummerMissingQuestion, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        // Edit a copy: if saving fails, the settings in memory stay as they are on disk.
        var changed = JsonDataFile.Clone(_folder.Einstellungen);
        _panel.ApplyTo(changed);
        try
        {
            _folder.SaveEinstellungen(changed);
        }
        catch (DataFileException ex)
        {
            MessageBox.Show(this, UiText.DataFileNotWritable(ex.FilePath, ex.Message), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
