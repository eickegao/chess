
// This file has been generated by the GUI designer. Do not modify.
namespace GUI
{
	public partial class EngineStrengthDialog
	{
		private global::Gtk.HBox hbox1;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.RadioButton DepthButton;
		
		private global::Gtk.RadioButton TimeButton;
		
		private global::Gtk.SpinButton StrengthValue;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget GUI.EngineStrengthDialog
			this.WidthRequest = 250;
			this.HeightRequest = 125;
			this.Name = "GUI.EngineStrengthDialog";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child GUI.EngineStrengthDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(17));
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.DepthButton = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("Depth"));
			this.DepthButton.CanFocus = true;
			this.DepthButton.Name = "DepthButton";
			this.DepthButton.Active = true;
			this.DepthButton.DrawIndicator = true;
			this.DepthButton.UseUnderline = true;
			this.DepthButton.Group = new global::GLib.SList (global::System.IntPtr.Zero);
			this.vbox2.Add (this.DepthButton);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.DepthButton]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.TimeButton = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("Time (seconds)"));
			this.TimeButton.CanFocus = true;
			this.TimeButton.Name = "TimeButton";
			this.TimeButton.DrawIndicator = true;
			this.TimeButton.UseUnderline = true;
			this.TimeButton.Group = this.DepthButton.Group;
			this.vbox2.Add (this.TimeButton);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.TimeButton]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			this.hbox1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox2]));
			w4.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.StrengthValue = new global::Gtk.SpinButton (1, 1000, 1);
			this.StrengthValue.CanFocus = true;
			this.StrengthValue.Name = "StrengthValue";
			this.StrengthValue.Adjustment.PageIncrement = 10;
			this.StrengthValue.ClimbRate = 1;
			this.StrengthValue.Numeric = true;
			this.StrengthValue.Value = 1;
			this.hbox1.Add (this.StrengthValue);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.StrengthValue]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			w1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox1]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Internal child GUI.EngineStrengthDialog.ActionArea
			global::Gtk.HButtonBox w7 = this.ActionArea;
			w7.Name = "dialog1_ActionArea";
			w7.Spacing = 10;
			w7.BorderWidth = ((uint)(5));
			w7.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w8 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonCancel]));
			w8.Expand = false;
			w8.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w9 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonOk]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 250;
			this.DefaultHeight = 125;
			this.Show ();
		}
	}
}
