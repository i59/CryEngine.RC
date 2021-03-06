﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CryEngine.RC.Frontend.Properties;
using ManagedFbx;

namespace CryEngine.RC.Frontend
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			uxEngineTextbox.Text = Settings.Default.ProjectPath;
		}

		private void SelectFile(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog() { Filter = "Autodesk FBX (*.fbx)|*.fbx" };

			if(dialog.ShowDialog() == DialogResult.OK)
			{
				uxSourceTextbox.Text = dialog.FileName;
			}
		}

		private void SelectFolder(object sender, EventArgs e)
		{
			var dialog = new FolderBrowserDialog();

			if(dialog.ShowDialog() == DialogResult.OK)
			{
				Settings.Default.ProjectPath = dialog.SelectedPath;
				Settings.Default.Save();
				uxEngineTextbox.Text = dialog.SelectedPath;
			}
		}

		private void Export(object sender, EventArgs e)
		{
			Settings.Default.Save();

			if(!File.Exists(Path.Combine(Settings.Default.ProjectPath, "Bin32", "rc", "rc.exe")))
			{
				MessageBox.Show("Invalid project folder specified, couldn't locate the resource compiler.");
				return;
			}

			if(!File.Exists(uxSourceTextbox.Text) || new FileInfo(uxSourceTextbox.Text).Extension.ToLower() != ".fbx")
			{
				MessageBox.Show("Nonexistent file specified, or file is not an FBX file.");
				return;
			}

			var dialog = new SaveFileDialog { Filter = "CryENGINE Model (*.cgf)|*.cgf" };

			if(dialog.ShowDialog() == DialogResult.OK)
			{
				var scene = Scene.Import(uxSourceTextbox.Text);
				var node = scene.RootNode.ChildNodes.FirstOrDefault(n => n.Attributes.Any(a => a.Type == NodeAttributeType.Mesh));

				if(node == default(SceneNode))
				{
					MessageBox.Show("No mesh found in the FBX scene.");
					return;
				}

				var writer = new StringWriter();
				Log.Write = writer.WriteLine;

				var output = dialog.FileName.ToLower();
				var dae = output.Replace(".cgf", ".dae");

				FbxConverter.ToCollada(node.Mesh, dae);
				ColladaConverter.CEPath = Settings.Default.ProjectPath;
				ColladaConverter.ToCgf(dae);

				File.Delete(dae);
				File.Delete(dae + ".rcdone");
				
				uxLog.Text = writer.ToString();
				var selected = ActiveControl;
				ActiveControl = uxLog;
				uxLog.SelectionStart = 0;
				uxLog.ScrollToCaret();
				ActiveControl = selected;
			}
		}
	}
}
