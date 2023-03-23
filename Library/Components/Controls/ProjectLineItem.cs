using System.IO;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

public partial class ProjectLineItem : Control, IProjectIcon
{
	#region Signals
	[Signal] public delegate void FavoriteClickedEventHandler(ProjectLineItem pli, bool value);
	[Signal] public delegate void ClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightDoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragStartedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragEndedEventHandler(ProjectLineItem pli);
	#endregion
	
	#region Quick Create
	public static ProjectLineItem FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/ProjectLineItem.tscn");
		return scene.Instantiate<ProjectLineItem>();
	}
	#endregion
	
	#region Node Paths
	[NodePath] private TextureRect _projectIcon;
	[NodePath] private RichTextLabel _projectName;
	[NodePath] private Label _projectDesc;
	[NodePath] private Label _projectLoc;
	[NodePath] private Label _godotVersionDisplay;
	[NodePath] private Button _heart;
	#endregion
	
	#region Resources
	[Resource("res://Assets/Icons/svg/missing_icon.svg")] private Texture2D _missingIcon = null;
	[Resource("res://Assets/Icons/png/default_project_icon.png")] private Texture2D _defaultProjectIcon = null;
	#endregion
	
	#region Private Variables
	private GodotVersion _godotVersion;
	private ProjectFile _projectFile;
	private ShaderMaterial _shader;
	#endregion
	
	#region Public Properties
	public bool MissingProject { get; set; } = false;

	public GodotVersion GodotVersion
	{
		get => _godotVersion;
		set
		{
			_godotVersion = value;
			if (_godotVersionDisplay == null) return;
			_godotVersionDisplay.Text = value is null ? "Unknown" : $"Godot {_godotVersion.Tag}";
		}
	}

	public ProjectFile ProjectFile
	{
		get => _projectFile;
		set
		{
			if (_projectFile != null)
				_projectFile.ProjectChanged -= UpdateUI;
			
			_projectFile = value;

			if (_projectName is null) return;

			value.ProjectChanged += UpdateUI;
			
			UpdateUI();
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		GodotVersion = _godotVersion;
		ProjectFile = _projectFile;
		_shader = (ShaderMaterial)_heart.Material.Duplicate();
		_heart.Material = _shader;
		_shader.SetShaderParameter("s", _projectFile.Favorite ? 1.0f : 0.0f);
		_shader.SetShaderParameter("v", _projectFile.Favorite ? 1.0f : 0.5f);
		_heart.Toggled += (toggle) =>
		{
			_shader.SetShaderParameter("s", toggle ? 1.0f : 0.0f);
			_shader.SetShaderParameter("v", toggle ? 1.0f : 0.5f);
			EmitSignal(ProjectLineItem.SignalName.FavoriteClicked, this, toggle);
		};
		GuiInput += HandleGuiInput;
	}

	// public override bool _CanDropData(Vector2 atPosition, Variant data)
	// {
	// 	return GetParent().GetParent<CategoryList>()._CanDropData(atPosition, data);
	// }
	//
	// public override void _DropData(Vector2 atPosition, Variant data)
	// {
	// 	GetParent().GetParent<CategoryList>()._DropData(atPosition, data);
	// }
	//
	// public override Variant _GetDragData(Vector2 atPosition)
	// {
	// 	Dictionary<string, Node> data = new Dictionary<string, Node>();
	// 	
	// 	if (GetParent().GetParent() is not CategoryList)
	// 		return data;
	//
	// 	data["source"] = this;
	// 	data["parent"] = GetParent().GetParent();
	// 	var preview = FromScene();
	// 	preview.ProjectFile = ProjectFile;
	// 	preview.GodotVersion = GodotVersion;
	// 	var notifier = new VisibleOnScreenNotifier2D();
	// 	preview.AddChild(notifier);
	// 	notifier.ScreenEntered += () => EmitSignal(SignalName.DragStarted, this);
	// 	notifier.ScreenExited += () => EmitSignal(SignalName.DragEnded, this);
	// 	SetDragPreview(preview);
	// 	data["preview"] = preview;
	// 	return data;
	// }


	#endregion
	
	#region Godot Event Handlers

	void HandleGuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton inputEventMouseButton) return;

		switch (inputEventMouseButton.ButtonIndex)
		{
			case MouseButton.Left when inputEventMouseButton.DoubleClick:
				EmitSignal(SignalName.DoubleClicked, this);
				break;
			case MouseButton.Left:
				SelfModulate = Colors.White;
				EmitSignal(SignalName.Clicked, this);
				break;
			case MouseButton.Right when inputEventMouseButton.DoubleClick:
				EmitSignal(SignalName.RightDoubleClicked, this);
				break;
			case MouseButton.Right:
				SelfModulate = Colors.White;
				EmitSignal(SignalName.RightClicked, this);
				break;
			default:
				return;
		}
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Private Functions

	private void UpdateUI()
	{
		MissingProject = !File.Exists(ProjectFile.Location);
		_projectName.Text = ProjectFile.Name;
		_projectDesc.Text = ProjectFile.Description;
		_projectLoc.Text = MissingProject ? "Unknown Location" : ProjectFile.Location.GetBaseDir();
		_heart.ButtonPressed = ProjectFile.Favorite;

		if (MissingProject)
			_projectIcon.Texture = _missingIcon;
		else
		{
			var file = ProjectFile.Location.GetResourceBase(ProjectFile.Icon);
			_projectIcon.Texture = !File.Exists(file) ? _defaultProjectIcon : Util.LoadImage(file);
		}
	}
	#endregion
}