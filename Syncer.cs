using System;
using System.Linq;

namespace SaveBackup
{
	using System.ComponentModel;
	using System.Data;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using MyLibrary.FileSystemExt;


	class BoundsEventArgs:EventArgs
	{
		BoundsEventArgs()
		{
			_min = 0;
			_max = 0;
		}
		public BoundsEventArgs(int min, int max)
			: this()
		{
			Min = min;
			Max = max;
		}

		private int _min;
		private int _max;
		public int Min
		{
			get { return _min; }
			set
			{
				if (value > _max)
					_min = _max;
				else
				{
					_min = value;
				}
			}
		}
		public int Max
		{
			get { return _max; }
			set
			{
				if (value < _min)
					_max = _min;
				else
					_max = value;
			}
		}
	}

	class Syncer
	{

		public Syncer()
		{
			_comparingFiles = CompareFilesByTimeModifed;
		}

		public event EventHandler<BoundsEventArgs> StartBackuping;
		public event EventHandler CountEntrysProcessedChanged;
		public event EventHandler FinishedBackuping;

		#region Function Backup
		// 		public void Backup(/*IEnumerable<*/SaveDataSet.SaveDataTable/*>*/ listSaves, string backupFolder)
// 		{
// 			Regex regex = new Regex( "%.*%" );
// 			foreach (var listSave in listSaves)
// 			{
// 				MatchCollection matchCollection = regex.Matches( listSave.Path);
// 				string PathToDirectory = "";
// 				for (int i = 0; i < matchCollection.Count; i += 1)
// 				{
// 					string environment = matchCollection[i].Value;//listSave.Path.Substring( matchCollection[i].Index + 1,
// 					                       //                       matchCollection[i]. - matchCollection[i + 1] - 1 );
// 					PathToDirectory = Environment.GetEnvironmentVariable( environment);
// 				}
// 				//listSave;
// 			}
// 			//SetBoundsEventHandler( this, new BoundsEventArgs( 0, 100 ) );
// 		}
		#endregion

		private void CopyDirectory( string sourceDir, string destDir )
		{
			if (!Directory.Exists(sourceDir))
				return;
			if (!Directory.Exists( destDir))
				Directory.CreateDirectory( destDir);
			IEnumerable<string> listNameDirectores = Directory.EnumerateDirectories( sourceDir);
			foreach (var nameDirectore in listNameDirectores)
			{
				CopyDirectory(nameDirectore, PathCombine.Combine( destDir, Path.GetFileName( nameDirectore ) ) );
			}
			IEnumerable<string> listNameFiles = Directory.EnumerateFiles( sourceDir);
			foreach (var nameFile in listNameFiles)
			{
				CopyFile( nameFile, PathCombine.Combine( destDir, Path.GetFileName( nameFile ) ) );
			}
		}

		public delegate bool Overwriting( string sourceFile, string overwritingFile );
		Overwriting _comparingFiles ; 

		private bool CompareFilesByTimeModifed(string sourceFile, string detstFile)
		{
			if (File.GetLastWriteTime(sourceFile) < File.GetLastWriteTime(detstFile))
				return false;
			return true;
		}

		private void CopyFile( string sourceFile, string destFile )
		{
			if (!File.Exists( sourceFile))
				return;
			if (!File.Exists(destFile))
			{
				File.Copy( sourceFile, destFile );
			}
			else
			{
				if (_comparingFiles(sourceFile, destFile))
					File.Copy( sourceFile, destFile, true );
			}
		}

		public void Backup(IEnumerable<SaveDataSet.SaveRow> selectSave, string backupFolder, BackgroundWorker backgroundWorker, DoWorkEventArgs e)
		{
		//	backgroundWorker.ReportProgress( 0 );
			int curent = 0;
			int allEntry = selectSave.Count();
			foreach (var listSave in selectSave)
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
					return;
				}
				CopyDirectory(GetPathWithoutEnvironment(listSave.Path), PathCombine.Combine(backupFolder, Environment.UserName, listSave.Path));
				curent++;
				int progress = (int) ((float) curent / (float) allEntry * 100 );
				backgroundWorker.ReportProgress( progress );
			}
		}

		public void Backup(IEnumerable<SaveDataSet.SaveRow> selectSave, string backupFolder)
		{
			StartBackuping(this, new BoundsEventArgs( 0, selectSave.Count() ));
			foreach (var listSave in selectSave)
			{
				CopyDirectory( GetPathWithoutEnvironment( listSave.Path ), PathCombine.Combine(backupFolder, Environment.UserName, listSave.Path) ) ;
				CountEntrysProcessedChanged(this, new EventArgs());
			}
			FinishedBackuping( this, new EventArgs() );
		}

		public IEnumerable<SaveDataSet.SaveRow> AnalizeBackupStore(EnumerableRowCollection<SaveDataSet.SaveRow> listSaves, string pathBackupStore)
		{
			List<SaveDataSet.SaveRow> returnList = new List<SaveDataSet.SaveRow>();
			foreach (var listSave in listSaves)
			{
				if (Directory.Exists(PathCombine.Combine(pathBackupStore, Environment.UserName, listSave.Path)))
					returnList.Add(listSave);
			}
			return returnList;
		}

		private string GetPathWithoutEnvironment(string path)
		{
			Regex regex = new Regex("%.*?%");
			MatchCollection matchCollection = regex.Matches(path);
			string pathToDirectory = "";
			foreach (Match matchValue in matchCollection)
			{
				string environment = matchValue.Value.Substring(1, matchValue.Value.Length - 2);
				pathToDirectory = Environment.GetEnvironmentVariable(environment);
				pathToDirectory += PathCombine.Separator;
			}
			if (0 < matchCollection.Count)
				return PathCombine.Combine( pathToDirectory, path.Substring( path.LastIndexOf( '%' ) + 2 ) );
			return path;
		}

		public void Restore(IEnumerable<SaveDataSet.SaveRow> listexistSaves, string pathBackupStore )
		{
			foreach (var listexistSave in listexistSaves)
			{
				CopyDirectory( PathCombine.Combine(pathBackupStore, Environment.UserName, listexistSave.Path), 
					GetPathWithoutEnvironment( listexistSave.Path ));
			}
		}

		public Overwriting CompareintFilesDelegate
		{
			private get { return _comparingFiles; }
			set { _comparingFiles = value; }
		}
	}
}
