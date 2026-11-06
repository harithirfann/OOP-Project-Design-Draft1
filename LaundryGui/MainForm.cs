#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using LaundryLib;

namespace LaundryGui
{
    public class MainForm : Form
    {
        // ---- model ----
        private readonly LaundryRoom room;
        private readonly List<User> users = new List<User>();
        // The library keeps "which machine type is this queued user waiting for" private,
        // so the GUI mirrors it (set when Book returns null, cleared when Book succeeds).
        private readonly Dictionary<User, string> waitingType = new Dictionary<User, string>();

        // ---- controls ----
        private readonly ListView lvMachines = MakeList(
            "Id|60", "Type|70", "Status|90", "Price (RM)|80", "Cycle (min)|80", "Spec|90");
        private readonly ListView lvBookings = MakeList(
            "#|35", "User|110", "Machine|65", "Started|70", "Ready at|65", "Paid|110", "Status|70");
        private readonly ListView lvQueue = MakeList(
            "Pos|45", "User|140", "Waiting for|100");
        private readonly TextBox txtLog = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            BackColor = Color.White
        };

        private readonly ComboBox cboUser = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240, FormattingEnabled = true };
        private readonly ComboBox cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        private readonly ComboBox cboPay = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        private readonly RadioButton radioSms = new RadioButton { Text = "SMS", Checked = true, AutoSize = true };
        private readonly RadioButton radioEmail = new RadioButton { Text = "Email", AutoSize = true };

        public MainForm()
        {
            room = new LaundryRoom("Village 4 Laundry Room", new GuiNotifier(Log, false));

            // seed data (same as the console demo)
            room.AddMachine(new Washer("W1", 4.00, 8));
            room.AddMachine(new Washer("W2", 4.00, 10));
            room.AddMachine(new Dryer("D1", 3.00, 60));
            users.Add(new Student("U01", "Ali", "ali@utp.edu.my", "011-1111111", "22001234", "V4"));
            users.Add(new Student("U02", "Amir", "amir@utp.edu.my", "012-2222222", "22003456", "V4"));
            users.Add(new Student("U03", "Siti", "siti@utp.edu.my", "013-3333333", "22005678", "V5"));
            users.Add(new Staff("S01", "Mr. Raju", "raju@utp.edu.my", "014-4444444", "Facilities"));

            BuildUi();
            RefreshAll();
            Log("Ready. Notifications are going out by SMS.");
        }

        // =====================================================================
        //  UI construction
        // =====================================================================
        private void BuildUi()
        {
            Text = room.Name;
            Size = new Size(1180, 780);
            MinimumSize = new Size(960, 620);
            StartPosition = FormStartPosition.CenterScreen;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            // ---- row 0: admin bar ----
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            top.Controls.Add(MakeButton("Add Student", (s, e) => AddStudent()));
            top.Controls.Add(MakeButton("Add Staff", (s, e) => AddStaff()));
            top.Controls.Add(MakeButton("Add Washer", (s, e) => AddWasher()));
            top.Controls.Add(MakeButton("Add Dryer", (s, e) => AddDryer()));
            top.Controls.Add(MakeButton("Toggle Out-of-Order", (s, e) => ToggleOutOfOrder()));
            top.Controls.Add(new Label { Text = "Alerts via:", AutoSize = true, Margin = new Padding(20, 8, 0, 0) });
            radioSms.Margin = new Padding(3, 6, 3, 0);
            radioEmail.Margin = new Padding(3, 6, 3, 0);
            radioSms.CheckedChanged += OnNotifierChanged;
            radioEmail.CheckedChanged += OnNotifierChanged;
            top.Controls.Add(radioSms);
            top.Controls.Add(radioEmail);
            root.Controls.Add(top, 0, 0);
            root.SetColumnSpan(top, 2);

            // ---- row 1: booking bar ----
            cboType.Items.AddRange(new object[] { "Washer", "Dryer" });
            cboPay.Items.AddRange(new object[] { "Cash", "Card", "E-Wallet" });
            cboType.SelectedIndex = 0;
            cboPay.SelectedIndex = 0;
            cboUser.Format += (s, e) =>
            {
                if (e.ListItem is User u) e.Value = Describe(u);
            };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            bar.Controls.Add(new Label { Text = "User:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            bar.Controls.Add(cboUser);
            bar.Controls.Add(new Label { Text = "Machine:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) });
            bar.Controls.Add(cboType);
            bar.Controls.Add(new Label { Text = "Payment:", AutoSize = true, Margin = new Padding(10, 8, 0, 0) });
            bar.Controls.Add(cboPay);
            var btnBook = MakeButton("Book", (s, e) => OnBook());
            btnBook.Width = 90;
            btnBook.AutoSize = false;
            bar.Controls.Add(btnBook);
            var btnFinish = MakeButton("Finish Selected Booking", (s, e) => OnFinish());
            btnFinish.Margin = new Padding(40, 3, 3, 3);
            bar.Controls.Add(btnFinish);
            root.Controls.Add(bar, 0, 1);
            root.SetColumnSpan(bar, 2);

            // ---- rows 2-3: the four panels ----
            root.Controls.Add(MakeGroup("Machines", lvMachines), 0, 2);
            root.Controls.Add(MakeGroup("Bookings", lvBookings), 1, 2);
            root.Controls.Add(MakeGroup("Waiting queue", lvQueue), 0, 3);
            root.Controls.Add(MakeGroup("Notifications", txtLog), 1, 3);

            Controls.Add(root);
        }

        // each column is written as "Title|width"
        private static ListView MakeList(params string[] cols)
        {
            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false
            };
            foreach (string c in cols)
            {
                string[] parts = c.Split('|');
                lv.Columns.Add(parts[0], int.Parse(parts[1]));
            }
            return lv;
        }

        private static GroupBox MakeGroup(string title, Control content)
        {
            var g = new GroupBox { Text = title, Dock = DockStyle.Fill };
            content.Dock = DockStyle.Fill;
            g.Controls.Add(content);
            return g;
        }

        private static Button MakeButton(string text, EventHandler onClick)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 28 };
            b.Click += onClick;
            return b;
        }

        // =====================================================================
        //  Actions
        // =====================================================================
        private void OnBook()
        {
            User user = cboUser.SelectedItem as User;
            string type = cboType.SelectedItem as string;
            string pay = cboPay.SelectedItem as string;
            if (user == null || type == null || pay == null)
            {
                Warn("Pick a user, a machine type and a payment method first.");
                return;
            }

            try
            {
                Booking b = room.Book(user, type, pay);
                if (b == null) waitingType[user] = type.ToLowerInvariant(); // got queued
                else waitingType.Remove(user);                               // got a machine
            }
            catch (InvalidOperationException ex)
            {
                Warn(ex.Message);
            }
            RefreshAll();
        }

        private void OnFinish()
        {
            Booking b = SelectedTag<Booking>(lvBookings);
            if (b == null) { Warn("Select a booking in the Bookings list first."); return; }
            if (b.Done) { Warn($"Booking #{b.BookingNo} is already finished."); return; }

            room.FinishBooking(b);
            RefreshAll();
        }

        private void ToggleOutOfOrder()
        {
            Machine m = SelectedTag<Machine>(lvMachines);
            if (m == null) { Warn("Select a machine in the Machines list first."); return; }
            if (m.Status == MachineStatus.InUse) { Warn($"{m.Id} is in use. Finish its booking first."); return; }

            m.Status = m.Status == MachineStatus.OutOfOrder ? MachineStatus.Available : MachineStatus.OutOfOrder;
            Log($"Machine {m.Id} is now {m.Status}.");
            RefreshAll();
        }

        private void OnNotifierChanged(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;
            if (!rb.Checked) return; // ignore the radio that just got unchecked
            room.Notifier = new GuiNotifier(Log, radioEmail.Checked);
            Log("Notifications now go out by " + (radioEmail.Checked ? "EMAIL." : "SMS."));
        }

        // ---- add users / machines ----
        private void AddStudent()
        {
            string[] v = Prompt("Add Student", "ID", "Name", "Email", "Phone", "Matric No", "Hostel");
            if (v == null) return;
            if (!ValidUserId(v[0], v[1])) return;
            users.Add(new Student(v[0], v[1], v[2], v[3], v[4], v[5]));
            RefreshAll();
        }

        private void AddStaff()
        {
            string[] v = Prompt("Add Staff", "ID", "Name", "Email", "Phone", "Department");
            if (v == null) return;
            if (!ValidUserId(v[0], v[1])) return;
            users.Add(new Staff(v[0], v[1], v[2], v[3], v[4]));
            RefreshAll();
        }

        private void AddWasher()
        {
            string[] v = Prompt("Add Washer", "ID", "Price (RM)", "Capacity (kg)");
            if (v == null) return;
            if (!ValidMachineId(v[0])) return;
            if (!double.TryParse(v[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double price) || price < 0)
            { Warn("Price must be a number, 0 or more (e.g. 4.00)."); return; }
            if (!double.TryParse(v[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double kg) || kg <= 0)
            { Warn("Capacity must be a number above 0 (e.g. 8)."); return; }
            room.AddMachine(new Washer(v[0], price, kg));
            RefreshAll();
        }

        private void AddDryer()
        {
            string[] v = Prompt("Add Dryer", "ID", "Price (RM)", "Temperature (C)");
            if (v == null) return;
            if (!ValidMachineId(v[0])) return;
            if (!double.TryParse(v[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double price) || price < 0)
            { Warn("Price must be a number, 0 or more (e.g. 3.00)."); return; }
            if (!int.TryParse(v[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int temp) || temp <= 0)
            { Warn("Temperature must be a whole number above 0 (e.g. 60)."); return; }
            room.AddMachine(new Dryer(v[0], price, temp));
            RefreshAll();
        }

        private bool ValidUserId(string id, string name)
        {
            if (id.Length == 0 || name.Length == 0) { Warn("ID and Name are required."); return false; }
            if (users.Any(u => u.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            { Warn($"A user with ID {id} already exists."); return false; }
            return true;
        }

        private bool ValidMachineId(string id)
        {
            if (id.Length == 0) { Warn("ID is required."); return false; }
            if (room.Machines.Any(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            { Warn($"A machine with ID {id} already exists."); return false; }
            return true;
        }

        // =====================================================================
        //  Refresh: rebuild every list from the model
        // =====================================================================
        private void RefreshAll()
        {
            RefreshMachines();
            RefreshBookings();
            RefreshQueue();
            RefreshUsers();
        }

        private void RefreshMachines()
        {
            Machine sel = SelectedTag<Machine>(lvMachines);
            lvMachines.BeginUpdate();
            lvMachines.Items.Clear();
            foreach (Machine m in room.Machines)
            {
                string spec = m is Washer w ? $"{w.CapacityKg} kg"
                            : m is Dryer d ? $"{d.Temperature} deg C"
                            : "";
                var item = new ListViewItem(new[]
                {
                    m.Id, m.GetType().Name, m.Status.ToString(),
                    m.Price.ToString("F2"), m.CycleMinutes().ToString(), spec
                });
                item.Tag = m;
                switch (m.Status)
                {
                    case MachineStatus.Available: item.BackColor = Color.Honeydew; break;
                    case MachineStatus.InUse: item.BackColor = Color.LemonChiffon; break;
                    default: item.BackColor = Color.MistyRose; break;
                }
                lvMachines.Items.Add(item);
                if (ReferenceEquals(m, sel)) item.Selected = true;
            }
            lvMachines.EndUpdate();
        }

        private void RefreshBookings()
        {
            Booking sel = SelectedTag<Booking>(lvBookings);
            lvBookings.BeginUpdate();
            lvBookings.Items.Clear();
            foreach (Booking b in room.Bookings)
            {
                var item = new ListViewItem(new[]
                {
                    b.BookingNo.ToString(),
                    b.User.Name,
                    b.Machine.Id,
                    b.StartTime.ToString("HH:mm:ss"),
                    b.EndTime().ToString("HH:mm"),
                    $"RM{b.Payment.Amount:F2} {b.Payment.Method}",
                    b.Done ? "Done" : "Running"
                });
                item.Tag = b;
                if (b.Done) item.ForeColor = Color.Gray;
                lvBookings.Items.Add(item);
                if (ReferenceEquals(b, sel)) item.Selected = true;
            }
            lvBookings.EndUpdate();
        }

        private void RefreshQueue()
        {
            lvQueue.BeginUpdate();
            lvQueue.Items.Clear();
            foreach (User u in room.Queue.Waiting)
            {
                waitingType.TryGetValue(u, out string type);
                var item = new ListViewItem(new[]
                {
                    room.Queue.Position(u).ToString(),
                    u.Name,
                    type ?? "?"
                });
                item.Tag = u;
                lvQueue.Items.Add(item);
            }
            lvQueue.EndUpdate();
        }

        private void RefreshUsers()
        {
            int idx = cboUser.SelectedIndex;
            cboUser.Items.Clear();
            foreach (User u in users) cboUser.Items.Add(u);
            if (cboUser.Items.Count == 0) return;
            cboUser.SelectedIndex = (idx >= 0 && idx < cboUser.Items.Count) ? idx : 0;
        }

        // =====================================================================
        //  Helpers
        // =====================================================================
        private static string Describe(User u)
            => $"{u.Name} ({u.GetType().Name}, {u.ActiveBookings}/{u.MaxBookings()} active)";

        private static T SelectedTag<T>(ListView lv) where T : class
            => lv.SelectedItems.Count > 0 ? lv.SelectedItems[0].Tag as T : null;

        private void Log(string message)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}");
        }

        private void Warn(string message)
        {
            Log("! " + message);
            MessageBox.Show(this, message, "Laundry Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Small modal dialog: one labelled text box per label. Returns null if cancelled.
        private static string[] Prompt(string title, params string[] labels)
        {
            using (var f = new Form())
            {
                f.Text = title;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowInTaskbar = false;
                f.ClientSize = new Size(370, 20 + labels.Length * 32 + 55);

                var boxes = new TextBox[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    f.Controls.Add(new Label { Text = labels[i], Left = 12, Top = 23 + i * 32, Width = 130 });
                    boxes[i] = new TextBox { Left = 150, Top = 20 + i * 32, Width = 205 };
                    f.Controls.Add(boxes[i]);
                }

                int y = 20 + labels.Length * 32 + 10;
                var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 200, Top = y, Width = 75 };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 280, Top = y, Width = 75 };
                f.Controls.Add(ok);
                f.Controls.Add(cancel);
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                if (f.ShowDialog() != DialogResult.OK) return null;
                return boxes.Select(b => b.Text.Trim()).ToArray();
            }
        }
    }
}
