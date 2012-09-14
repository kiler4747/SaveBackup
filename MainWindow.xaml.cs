namespace SaveBackup
{
	#region

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Threading;
	using SaveBackup.SaveDataSetTableAdapters;

	#endregion

	/// <summary>
	/// 	Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SaveDataSet _saveDataSet;
		private SaveTableAdapter _saveDataSetSaveTableAdapter;
		private Syncer _syncer;
		private string _backupDir                   = "D:\\Backup\\";
		private readonly string _nameDownloadedFile = "Saves.mdf";
		private string _uri                         = "https://www.dropbox.com/s/1q1btrcpirpj1r2/Save.mdf?dl=1";
		private UpdateDataBase _updateDataBaseDialog;
		private List<SaveDataSet.SaveRow> existSaveList;

#if Paralel
		private BackgroundWorker _backgroundWorker;
#endif

		public MainWindow()
			: base()
		{
			InitializeComponent();
			_updateDataBaseDialog = null;
			_syncer = new Syncer();
			InicializeBackgroundWorker();
		}

		void InicializeBackgroundWorker()
		{
#if Paralel
			_backgroundWorker                            = new BackgroundWorker();
			_backgroundWorker.WorkerReportsProgress      = true;
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.ProgressChanged           += BackgroundWorkerProgressChanged;
			_backgroundWorker.RunWorkerCompleted        += BackgroundWorkerRunWorkerCompleted;
			_backgroundWorker.DoWork                    += BackgroundWorkerDoWork;
#else
			_syncer.CountEntrysProcessedChanged += (sender, args) =>
			                                       	{
														ProgressBackuping.Value++;
			                                       	};
#endif
		}

		void UnblocktUserControlInterface()
		{
			SelectAll.IsEnabled          = true;
			UnselectAll.IsEnabled        = true;
			BackupSelected.Visibility    = Visibility.Visible;
			CancelBackup.Visibility      = Visibility.Collapsed;
			ProgressBackuping.Visibility = Visibility.Collapsed;
		}

		void BlockUserControlInterface()
		{
			SelectAll.IsEnabled          = false;
			UnselectAll.IsEnabled        = false;
			BackupSelected.Visibility    = Visibility.Collapsed;
			CancelBackup.Visibility      = Visibility.Visible;
			ProgressBackuping.Visibility = Visibility.Visible;

		}

#if Paralel
		void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			if (((sender as BackgroundWorker).CancellationPending == true))
			{
				e.Cancel = true;
				return;
			}
			var selectSave = from saveDataTable in _saveDataSet.Save where saveDataTable.Check select saveDataTable;
			_syncer.Backup(selectSave, _backupDir, sender as BackgroundWorker, e);
		}
		void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			UnblocktUserControlInterface();
		}
		void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ProgressBackuping.Value = e.ProgressPercentage;
		}
#endif


		private void InicializeDataSet()
		{
			_saveDataSet = ((SaveDataSet)(FindResource("saveDataSet")));
			// Load data into the table Save. You can modify this code as needed.
			_saveDataSetSaveTableAdapter = new SaveTableAdapter();
			_saveDataSetSaveTableAdapter.Fill(_saveDataSet.Save);
			CollectionViewSource saveViewSource = ((CollectionViewSource)(FindResource("saveViewSource")));
			saveViewSource.View.MoveCurrentToFirst();
#if DEBUG
			_saveDataSetSaveTableAdapter.Connection.Close();
			_saveDataSet.Dispose();
			_saveDataSetSaveTableAdapter.Dispose();
			((CollectionViewSource) (FindResource( "saveViewSource" ))).Source = null;
			_saveDataSet = null;
			_saveDataSetSaveTableAdapter = null;
			saveViewSource.Source = null;
			saveViewSource = null;
#endif
			_saveDataSet.Save.RowChanged += DataSetRowEditedEventHandler;
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			InicializeDataSet();
			SaveBackup.SaveEntities saveEntities = new SaveBackup.SaveEntities();
			// Load data into Saves. You can modify this code as needed.
			System.Windows.Data.CollectionViewSource savesViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("savesViewSource")));
			System.Data.Objects.ObjectQuery<SaveBackup.Save> savesQuery = this.GetSavesQuery(saveEntities);
			savesViewSource.Source = savesQuery.Execute(System.Data.Objects.MergeOption.AppendOnly);
		}

		private void DataSetRowEditedEventHandler( object sender , DataRowChangeEventArgs e)
		{
			_saveDataSetSaveTableAdapter.Update(_saveDataSet);
			_saveDataSet.AcceptChanges();
		}

		private void SaveDataGridCellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
		{
			
		}

		private void SelectAllClick(object sender, RoutedEventArgs e)
		{
			foreach (var save in _saveDataSet.Save)
			{
				save.Check = true;
			}
			//_saveDataSet.AcceptChanges();
			
			//_saveDataSetSaveTableAdapter.Fill();
		}

		private void UnselectAllClick(object sender, RoutedEventArgs e)
		{
			foreach (var save in _saveDataSet.Save)
			{
				save.Check = false;
			}
		}

		private void BackupSelectedClick(object sender, RoutedEventArgs e)
		{
			BlockUserControlInterface();
#if Paralel
			_backgroundWorker.RunWorkerAsync();
#else
			var selectSave = from saveDataTable in _saveDataSet.Save where saveDataTable.Check select saveDataTable;
			ProgressBackuping.Maximum = selectSave.Count();
			_syncer.Backup(selectSave , _backupDir);
			UnblocktUserControlInterface();
#endif
		}

		private void UpdateFinishing(object sender, EventArgs e)
		{
			try
			{

// 							_saveDataSetSaveTableAdapter.Connection.Close();
// 								//			_saveDataSetSaveTableAdapter.Adapter.Dispose();
// 								//			_saveDataSetSaveTableAdapter.Dispose();
// 								//			_saveDataSetSaveTableAdapter = 
// 								//			File.Replace( "Save.mdf", _nameDownloadedFile, "Save.mdf.bak" );
// 								//			InicializeDataSet();
// 								//			_saveDataSetSaveTableAdapter.Connection.Open();
// 				
// 								//			SaveDataSet newDataBaseDataSet = new SaveDataSet();
// 								//			SaveTableAdapter newDataBaseAdapter = new SaveTableAdapter();
// 								//			newDataBaseAdapter.Connection.ChangeDatabase( _nameDownloadedFile );
// 								_saveDataSet.Save.BeginLoadData();
// 							string queryString = @"SELECT * FROM [Save];";
// 				
// 							using (SqlConnection connection =
// 									   new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\"+_nameDownloadedFile+";Integrated Security=True;Connect Timeout=300;User Instance=True"))
// 							{
// 								using (SqlCommand command =
// 									new SqlCommand(queryString, connection))
// 								{
// 									connection.Open();
// 									SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
// 									SqlDataReader reader = command.ExecuteReader();
// 									reader.Close();
// 									SaveDataSet updateDataSet = new SaveDataSet();
// 									// Call Read before accessing data.
// 									adapter.Fill( updateDataSet );
// 									adapter.Update( updateDataSet);
// 									
// 									
// 									while (reader.Read())
// 									{
// 									}
// 									IDataReader datareader = new DataTableReader( updateDataSet.Save );
// 									_saveDataSet.Save.Load( datareader, LoadOption.OverwriteChanges);
// 				
// 									// Call Close when done reading.
// 								}
// 							}
// 				#if aa
// 								SqlConnection connection =
// 										new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\" + _nameDownloadedFile +
// 														   ";Integrated Security=True;Connect Timeout=300;User Instance=True");
// 								SqlDataAdapter NwindDA = new SqlDataAdapter("SELECT * FROM [Save]", connection);
// 								SqlCommandBuilder x = new SqlCommandBuilder(NwindDA);
// 								connection.Open();
// 								NwindDA.Update(_saveDataSet, "Save");
// 				#endif
// 								_saveDataSet.Save.EndLoadData();
// 								_saveDataSetSaveTableAdapter.Update(_saveDataSet);
// 						
				_saveDataSetSaveTableAdapter.Connection.Close();
				_saveDataSetSaveTableAdapter.Connection.Dispose();
				//File.Delete( "Save.mdf" );
//				_saveDataSetSaveTableAdapter.Connection.ConnectionString =
//						@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\" + _nameDownloadedFile +
//						";Integrated Security=True;Connect Timeout=300;User Instance=True";
				//_saveDataSetSaveTableAdapter.Connection.Open();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void UpdateDataBaseClick(object sender, RoutedEventArgs e)
		{
// 			_updateDataBaseDialog = new UpdateDataBase(_uri, _nameDownloadedFile);
// 			_updateDataBaseDialog.UpdateFinished += UpdateFinishing;
// 			_updateDataBaseDialog.Show();
			UpdateFinishing( null, new EventArgs() );
		}

		private IEnumerable<SaveDataSet.SaveRow> _qure;

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			_qure = _syncer.AnalizeBackupStore( from s in _saveDataSet.Save select s, _backupDir );
			IEnumerable<bool> b = new List<bool>();
			dataGrid1.ItemsSource = _qure;
		}

		private void button3_Click(object sender, RoutedEventArgs e)
		{
			_syncer.Restore( _qure, _backupDir );
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
#if Paralel
			_backgroundWorker.CancelAsync();
#endif
		}

		private System.Data.Objects.ObjectQuery<Save> GetSavesQuery(SaveEntities saveEntities)
		{
			// Auto generated code

			System.Data.Objects.ObjectQuery<SaveBackup.Save> savesQuery = saveEntities.Saves;
			// Returns an ObjectQuery.
			return savesQuery;
		}
	}
}