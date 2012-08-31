namespace SaveBackup
{
	#region

	using System;
	using System.ComponentModel;
	using System.IO;
	using System.Net;
	using System.Windows;

	#endregion

	/// <summary>
	/// 	Interaction logic for UpdateDataBase.xaml
	/// </summary>
	public partial class UpdateDataBase : Window
	{
		private readonly string _uri;
		private WebClient _client;
		private bool _downloading;
		private FileDownloaded _fileDownloaded;
		private string _nameDownloadedFile;

		public string NameDownloadedFile
		{
			get { return _nameDownloadedFile; }
			set { _nameDownloadedFile = value; }
		}

		private UpdateDataBase()
		{
			InitializeComponent();
			_uri = "";
			_nameDownloadedFile = "file.new";
			_fileDownloaded = NothingDelegate;
		}

		public UpdateDataBase( string uri, string nameDownloadedFile )
				: this()
		{
			_uri = uri ?? "";
			_nameDownloadedFile = nameDownloadedFile ?? "file.new";
		}

		public string Uri
		{
			get { return Uri; }
			set { Uri = value; }
		}

		public FileDownloaded FileDownload
		{
			get { return _fileDownloaded; }
			set { _fileDownloaded = value; }
		}

		private void NothingDelegate()
		{
		}

		public event EventHandler UpdateFinished;

		private void StartOrStopDownloading()
		{
			if (!_downloading)
			{
				_client = new WebClient();
				_client.DownloadProgressChanged += ClientDownloadProgressChanged;
				_client.DownloadFileCompleted += ClientDownloadDataCompleted;
				try
				{
					_client.DownloadFileAsync( new Uri( _uri ), _nameDownloadedFile );
					_downloading = true;
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				_client.CancelAsync();
			}
		}

		private void ClientDownloadDataCompleted( object sender, AsyncCompletedEventArgs e )
		{
			if (e.Cancelled)
			{
				downloadProgress.Value = 0;
				label1.Content = Properties.Resources.Canceled;
			}
			else if (e.Error != null)
			{
				downloadProgress.Value = 0;
				label1.Content = e.Error.Message;
			}
			else
			{
				downloadProgress.Value = 100;
				label1.Content = Properties.Resources.Done;
			}
			_client.Dispose();
			_client = null;
			_downloading = false;
			Update.Content = Properties.Resources.Update;
//			File.Create( _nameDownloadedFile).Write( e.Result, 0, e.Result.Length );
			MessageBox.Show( DateTime.Now.ToString() );
			UpdateFinished(null, new EventArgs());
		}

		private void ClientDownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
		{
			downloadProgress.Value = e.ProgressPercentage;
			label1.Content = string.Format( "{0:N0} / {1:N0} bytes received", e.BytesReceived, e.TotalBytesToReceive );
		}

		private void UpdateClick( object sender, RoutedEventArgs e )
		{
			StartOrStopDownloading();
			Update.Content = Properties.Resources.Cancel;
		}
	}

	public delegate void FileDownloaded();
}