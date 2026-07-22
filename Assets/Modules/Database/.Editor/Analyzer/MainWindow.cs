using EditorDatabase.Storage;
using EditorDatabase;
using Analyzer.Analyzer;

namespace Analyzer
{
    public partial class MainWindow : Form
    {
		private Database _database;

		public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs eventArgs)
        {
            ReloadDatabase();
        }

        private void OpenDatabase(string path)
        {
            try
            {
                _database = new Database(new DatabaseStorage(path));            
                console.AppendText("Database loaded\n");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " " + e.StackTrace);
            }
        }

        private void ammunitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            console.Clear();
            console.AppendText("Total score:\n");

			var statistics = new Analyzer.Statistics(_database);

			foreach (var item in statistics.GetCannonStats().ToArray().OrderBy(item => item.name))
            {
                console.AppendText(ExpandString(item.name, 24));
                console.AppendText(" id:" + item.id + "\t");
                console.AppendText(item.special ? "*" : " ");
                console.AppendText("lvl:" + item.level.ToString("000") + "\t");
                console.AppendText("s:" + item.size + "\t");
                console.AppendText("dps:" + item.dps.ToString("0.0") + "\t");
                console.AppendText("eps:" + item.eps.ToString("0.0") + "\t");
                console.AppendText("e2d:" + (item.dps / item.eps).ToString("0.0") + "\t");
                console.AppendText("rng:" + item.range.ToString("0.00") + (item.homing ? "*" : " ") + "\t");
                console.AppendText("score:" + item.score.ToString("0.0") + "\t");

                console.AppendText("\n");
            }
        }

        private static string ExpandString(string str, int length)
        {
            if (length <= str.Length) return str.Substring(0, length);
            return str.PadRight(length, ' ');
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            console.Clear();
            ReloadDatabase();
        }

        private void ReloadDatabase()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                OpenDatabase(args[1]);
            else
                OpenDatabase(Directory.GetCurrentDirectory());
        }

        private void deadLinksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            console.Clear();
            console.AppendText("Dead links:\n");

			var integrityChecker = new Analyzer.IntegrityChecker(_database);

            foreach (var item in integrityChecker.LookForDeadLinks())
                console.AppendText(item + "\n");
        }

        private void shipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
			console.Clear();

			var barrelAnalyzer = new ShipBarrelAnalyzer(_database);

			foreach (var item in barrelAnalyzer.AnalyzeShips())
				console.AppendText(item + "\n");
		}
	}
}
