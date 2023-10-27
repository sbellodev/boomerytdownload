using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Playlists;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using YoutubeExplode.Common;
using System.Windows.Forms;

namespace BoomerYTDownloader
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox playlistUrlTextBox;
        private Button downloadButton;
        private Button folderButton; // New button for choosing the destination folder
        private Label statusLabel;

        private FolderBrowserDialog folderBrowserDialog; // New FolderBrowserDialog control

        private string destinationFolder; // Variable to store the chosen destination folder

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "YouTube Downloader";

            // Create a TextBox for entering the playlist URL
            playlistUrlTextBox = new TextBox();
            playlistUrlTextBox.Location = new System.Drawing.Point(20, 20);
            playlistUrlTextBox.Size = new System.Drawing.Size(400, 25);
            this.Controls.Add(playlistUrlTextBox);

            // Create a Download button
            downloadButton = new Button();
            downloadButton.Text = "Download";
            downloadButton.Location = new System.Drawing.Point(440, 20);
            downloadButton.Size = new System.Drawing.Size(100, 25);
            downloadButton.Click += new System.EventHandler(this.downloadButton_Click); // Attach event handler
            this.Controls.Add(downloadButton);

            // Create a Choose Folder button
            folderButton = new Button();
            folderButton.Text = "Choose Folder";
            folderButton.Location = new System.Drawing.Point(560, 20);
            folderButton.Size = new System.Drawing.Size(120, 25);
            folderButton.Click += new System.EventHandler(this.folderButton_Click); // Attach event handler
            this.Controls.Add(folderButton);

            // Create a Label for displaying status messages
            statusLabel = new Label();
            statusLabel.Text = "Status:";
            statusLabel.Location = new System.Drawing.Point(20, 60);
            statusLabel.Size = new System.Drawing.Size(600, 25);
            this.Controls.Add(statusLabel);

            // Initialize the FolderBrowserDialog
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select Destination Folder";
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog.ShowNewFolderButton = true;
        }

        private void folderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                destinationFolder = folderBrowserDialog.SelectedPath;
            }
        }

        private async void downloadButton_Click(object sender, EventArgs e)
        {
            string playlistUrl = playlistUrlTextBox.Text;
            if (string.IsNullOrEmpty(playlistUrl))
            {
                statusLabel.Text = "Please enter a valid playlist URL.";
                return;
            }

            if (string.IsNullOrEmpty(destinationFolder))
            {
                statusLabel.Text = "Please choose a destination folder.";
                return;
            }

            try
            {
                var youtubeClient = new YoutubeClient();
                var videos = await youtubeClient.Playlists.GetVideosAsync(playlistUrl);

                // Show a loading message
                statusLabel.Text = "Downloading playlist. Please wait...";

                foreach (var video in videos)
                {
                    var videoUrl = $"https://www.youtube.com/watch?v={video.Id}";
                    var videoInfo = await youtubeClient.Videos.GetAsync(videoUrl);

                    var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoInfo.Id);
                    var audioStreamInfo = streamManifest.GetAudioOnlyStreams().FirstOrDefault();

                    if (audioStreamInfo != null)
                    {
                        var outputAudioPath = Path.Combine(destinationFolder, $"{videoInfo.Title}.mp3");

                        using (var stream = await youtubeClient.Videos.Streams.GetAsync(audioStreamInfo))
                        using (var output = File.Create(outputAudioPath))
                        {
                            await stream.CopyToAsync(output);
                        }

                        // Append the status message without overwriting previous messages
                        statusLabel.Text += $"Audio from video {videoInfo.Title} has been downloaded and saved as MP3\n";
                    }
                    else
                    {
                        // Append the status message without overwriting previous messages
                        statusLabel.Text += $"No audio stream available for video {videoInfo.Title}\n";
                    }
                }

                // Remove the loading message
                statusLabel.Text = "Playlist download complete!";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "An error occurred: " + ex.Message;
                //MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

    }
}
