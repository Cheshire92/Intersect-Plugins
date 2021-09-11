using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Cheshire.Plugins.Utilities.Client.Interface;

using Intersect;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Client.Plugins;
using Intersect.GameObjects.Maps;
using Intersect.Client.Framework.Entities;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Cheshire.Plugins.Utilities.Client.ContentManager;
using Intersect.Client.Framework.Content;
using Cheshire.Plugins.Client.Minimap.Configuration;

namespace Cheshire.Plugins.Client.Minimap
{
    public class Minimap
    { 
        private IClientPluginContext mContext;

        private Point mMinimapTileSize;

        private string mPluginDir;

        private GameRenderTexture mRenderTexture;

        private ImagePanel mMinimap;

        private ImagePanel mOverlay;

        private WindowControl mWindowControl;

        private Button mZoomInButton;

        private Button mZoomOutButton;

        private bool mRedrawMaps;

        private bool mRedrawEntities;

        private Dictionary<MapPosition, GameRenderTexture> mMinimapCache = new Dictionary<MapPosition, GameRenderTexture>();
        
        private Dictionary<MapPosition, GameRenderTexture> mEntityCache = new Dictionary<MapPosition, GameRenderTexture>();

        private Dictionary<MapPosition, Dictionary<Point, EntityInfo>> mEntityInfoCache = new Dictionary<MapPosition, Dictionary<Point, EntityInfo>>();

        private Dictionary<MapPosition, MapBase> mMapGrid = new Dictionary<MapPosition, MapBase>();

        private Dictionary<Guid, MapPosition> mMapPosition = new Dictionary<Guid, MapPosition>();

        private GameRenderTexture mWhiteTexture;

        private byte mZoomLevel;

        public Minimap(IClientPluginContext context, int tileSizeX, int tileSizeY, string pluginDir)
        {
            mContext = context;
            mMinimapTileSize = new Point(tileSizeX, tileSizeY);
            mPluginDir = pluginDir;
        }

        public void Initialize()
        {
            // Load our graphical assets into the engine!
            mContext.ContentManager.LoadAssets(Path.Combine(mPluginDir, "resources"), new List<ContentTypes>() { ContentTypes.Interface, ContentTypes.Miscellaneous });

            // Generate our GUI controls and load their layout.
            GenerateControls();
            mWindowControl.LoadJsonUI(Path.Combine(mPluginDir, "resources", "gui", "layouts", "game", "MinimapLayout.json"));
            mWindowControl.DisableResizing();

            // Generate some textures that we'll be using for rendering..
            mWhiteTexture = mContext.Graphics.CreateWhiteTexture();
            mRenderTexture = GenerateBaseRenderTexture();

            // Set our minimap background texture.
            mMinimap.Texture = mRenderTexture;
            mMinimap.SetTextureRect(0, 0, mRenderTexture.Width, mRenderTexture.Height);

            mZoomLevel = PluginSettings.Settings.DefaultZoom;
        }

        public void Update(IPlayer entity, IReadOnlyDictionary<Guid, IEntity> allEntities)
        {
            if (entity == null || entity.MapInstance == null)
            {
                return;
            }

            // Generate a brand new map grid that we'll use to create our minimap!
            var mapBase = MapBase.Get(entity.MapInstance.Id);
            var newGrid = CreateMapGridFromMap(mapBase);

            // Have we changed maps at all? If so we'll have to start some things from scratch!
            if (!newGrid.SequenceEqual(mMapGrid))
            {
                // Set our last map to our current map so we don't repeat this every update!
                mMapGrid = newGrid;

                // Now that that's been changed, reset our location cache.. Will make dealing with entity locations easier!
                mMapPosition.Clear();
                foreach (var map in mMapGrid)
                {
                    if (map.Value != null)
                    {
                        mMapPosition.Add(map.Value.Id, map.Key);
                    }
                }

                mRedrawMaps = true;
            }

            // check the same for entity locations!
            var newLocations = GenerateEntityInfo(allEntities, entity);
            if (newLocations != mEntityInfoCache)
            {
                mEntityInfoCache = newLocations;
                mRedrawEntities = true;
                
            }

            // Update our minimap display area
            var centerX = (mRenderTexture.Width / 3) + (entity.X * PluginSettings.Settings.MinimapTileSize.X);
            var centerY = (mRenderTexture.Height / 3) + (entity.Y * PluginSettings.Settings.MinimapTileSize.Y);
            var displayWidth = (int)(mRenderTexture.Width * (mZoomLevel / 100f));
            var displayHeight = (int)(mRenderTexture.Height * (mZoomLevel / 100f));

            var x = centerX - (displayWidth / 2);
            if (x + displayWidth > mRenderTexture.Width)
            {
                x = mRenderTexture.Width - displayWidth;
            }
            if (x < 0)
            {
                x = 0;
            }

            var y = centerY - (displayHeight / 2);
            if (y + displayHeight > mRenderTexture.Height)
            {
                y = mRenderTexture.Height - displayHeight;
            }
            if (y < 0)
            {
                y = 0;
            }

            mMinimap.SetTextureRect(x, y, displayWidth, displayHeight);
        }

        public void Draw()
        {
            // Clear the minimap texture so we can draw on it again.
            mRenderTexture.Clear(Color.Transparent);

            // Render our minimap again!
            foreach (var pos in mMapGrid.Keys)
            {
                // Do we need to actually redraw our maps?
                if (mRedrawMaps)
                {
                    GenerateMinimapCacheFor(pos);
                }

                if (mRedrawEntities)
                {
                    GenerateEntityCacheFor(pos);
                }

                // Draw our map to the base texture.
                DrawMinimapCacheToTexture(pos);
            }

            // Toggle some switches off so we don't redraw caches literally every frame.
            if (mRedrawMaps)
            {
                mRedrawMaps = false;
            }

            if (mRedrawEntities)
            {
                mRedrawEntities = false;
            }
        }

        private void GenerateMinimapCacheFor(MapPosition position)
        {
            if (!mMinimapCache.ContainsKey(position))
            {
                mMinimapCache.Add(position, null);
            }

            // Generate a new cache texture if it does not exist.
            if (mMinimapCache[position] == null)
            {
                mMinimapCache[position] = GenerateMapRenderTexture();
            }
            mMinimapCache[position].Clear(Color.Transparent);

            // Do we have an actual map to generate a minimap for? If not, leave it at a blank texture.
            if (mMapGrid[position] != null)
            {
                foreach (var layer in mContext.Options.MapOpts.Layers.All)
                {
                    // If this layer is not configured to render, skip it!
                    if (!PluginSettings.Settings.RenderLayers.Contains(layer))
                    {
                        continue;
                    }

                    // Go through each x and y coordinate on the map to attempt to render this layer's tiles.
                    for (var x = 0; x < mContext.Options.MapOpts.Width; x++)
                    {
                        for (var y = 0; y < mContext.Options.MapOpts.Height; y++)
                        {
                            // Is there a valid tileset to render on this map?
                            var curTile = mMapGrid[position].Layers[layer][x, y];
                            if (curTile.TilesetId != Guid.Empty)
                            {
                                // Attempt to load the tileset and texture for it, if they exist actually draw the CENTER pixel of the tile to the screen for color approximation!
                                // This method used to get the color of a pixel on the tile, but this was significantly slower than simply grabbing the pixel itself and drawing that.
                                var tileset = TilesetBase.Get(curTile.TilesetId);
                                var texture = mContext.ContentManager.Find<GameTexture>(ContentTypes.TileSet, tileset.Name);

                                if (texture != null)
                                {
                                    mContext.Graphics.DrawTexture(
                                        texture,
                                        curTile.X * mContext.Options.MapOpts.TileWidth + (mContext.Options.MapOpts.TileWidth / 2),
                                        curTile.Y * mContext.Options.MapOpts.TileWidth + (mContext.Options.MapOpts.TileWidth / 2),
                                        1,
                                        1,
                                        x * mMinimapTileSize.X, y * mMinimapTileSize.Y,
                                        mMinimapTileSize.X, mMinimapTileSize.Y,
                                        Color.White,
                                        mMinimapCache[position]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateEntityCacheFor(MapPosition position)
        {
            if (!mEntityCache.ContainsKey(position))
            {
                mEntityCache.Add(position, null);
            }

            // Generate a new cache texture if it does not exist, and then make it transparent!
            if (mEntityCache[position] == null)
            {
                mEntityCache[position] = GenerateMapRenderTexture();
            }
            mEntityCache[position].Clear(Color.Transparent);

            // Do we have an actual map to generate a minimap for? If not, leave it at a blank texture.
            if (mEntityInfoCache.ContainsKey(position) && mEntityInfoCache[position] != null)
            {
                foreach (var entity in mEntityInfoCache[position])
                {
                    var texture = mWhiteTexture as GameTexture;
                    var color = entity.Value.Color;

                    if (!string.IsNullOrWhiteSpace(entity.Value.Texture))
                    {
                        var found = mContext.ContentManager.Find<GameTexture>(ContentTypes.Miscellaneous, entity.Value.Texture);
                        if (found != null)
                        {
                            texture = found;
                            color = Color.White;
                        }
                    }

                    mContext.Graphics.DrawTexture(
                        texture,
                        0,
                        0,
                        texture.Width,
                        texture.Height,
                        entity.Key.X * mMinimapTileSize.X,
                        entity.Key.Y * mMinimapTileSize.Y,
                        mMinimapTileSize.X,
                        mMinimapTileSize.Y,
                        color,
                        mEntityCache[position], GameBlendModes.Add);
                }
                    
                
            }
        }

        private void DrawMinimapCacheToTexture(MapPosition position)
        {
            if (!mMinimapCache.ContainsKey(position))
            {
                return;
            }

            var x = 0;
            var y = 0;

            // * Internal Screaming *
            switch(position)
            {
                case MapPosition.TopLeft:
                    // Topleft is fine by default, ignore!
                    break;

                case MapPosition.TopMiddle:
                    x = mMinimapCache[position].Width;
                    break;

                case MapPosition.TopRight:
                    x = mMinimapCache[position].Width * 2;
                    break;

                case MapPosition.MiddleLeft:
                    y = mMinimapCache[position].Height;
                    break;

                case MapPosition.Middle:
                    x = mMinimapCache[position].Width;
                    y = mMinimapCache[position].Height;
                    break;

                case MapPosition.MiddleRight:
                    x = mMinimapCache[position].Width * 2;
                    y = mMinimapCache[position].Height;
                    break;

                case MapPosition.BottomLeft:
                    y = mMinimapCache[position].Height * 2;
                    break;

                case MapPosition.BottomMiddle:
                    x = mMinimapCache[position].Width;
                    y = mMinimapCache[position].Height * 2;
                    break;

                case MapPosition.BottomRight:
                    x = mMinimapCache[position].Width * 2;
                    y = mMinimapCache[position].Height * 2;
                    break;
            }

            if (mMinimapCache.ContainsKey(position))
            {
                mContext.Graphics.DrawTexture(mMinimapCache[position], 0, 0, mMinimapCache[position].Width, mMinimapCache[position].Height, x, y, mMinimapCache[position].Width, mMinimapCache[position].Height, Color.White, mRenderTexture);
            }

            if (mEntityCache.ContainsKey(position))
            {
                mContext.Graphics.DrawTexture(mEntityCache[position], 0, 0, mEntityCache[position].Width, mEntityCache[position].Height, x, y, mEntityCache[position].Width, mEntityCache[position].Height, Color.White, mRenderTexture);
            }
        }

        private Dictionary<MapPosition, MapBase> CreateMapGridFromMap(MapBase map)
        {
            // Create and fill our grid with the easily accessible data.
            var grid = new Dictionary<MapPosition, MapBase>();
            grid.Add(MapPosition.Middle, map);
            grid.Add(MapPosition.TopMiddle, MapBase.Get(map.Up));
            grid.Add(MapPosition.MiddleLeft, MapBase.Get(map.Left));
            grid.Add(MapPosition.MiddleRight, MapBase.Get(map.Right));
            grid.Add(MapPosition.BottomMiddle, MapBase.Get(map.Down));

            // Fill in the blanks that we need to figure out through other maps!
            grid.Add(MapPosition.TopLeft, MapBase.Get(grid[MapPosition.TopMiddle]?.Left ?? Guid.Empty));
            grid.Add(MapPosition.TopRight, MapBase.Get(grid[MapPosition.TopMiddle]?.Right ?? Guid.Empty));
            grid.Add(MapPosition.BottomLeft, MapBase.Get(grid[MapPosition.BottomMiddle]?.Left ?? Guid.Empty));
            grid.Add(MapPosition.BottomRight, MapBase.Get(grid[MapPosition.BottomMiddle]?.Right ?? Guid.Empty));
            return grid;
        }

        private Dictionary<MapPosition, Dictionary<Point, EntityInfo>> GenerateEntityInfo(IReadOnlyDictionary<Guid, IEntity> entities, IPlayer player)
        {
            var dict = new Dictionary<MapPosition, Dictionary<Point, EntityInfo>>();
            foreach(var entity in entities)
            {
                if (mMapPosition.ContainsKey(entity.Value.MapInstance.Id))
                {
                    // if entity is hidden for whatever reason, don't display!
                    if (entity.Value.IsHidden)
                    {
                        continue;
                    }

                    var map = mMapPosition[entity.Value.MapInstance.Id];
                    var color = Color.Transparent;
                    var texture = string.Empty;

                    // Get a render colour for this entity.. Force our own to be different!
                    // Force our own to be a different colour!
                    if (entity.Key == player.Id)
                    {
                        color = PluginSettings.Settings.Colors.MyEntity;
                        texture = PluginSettings.Settings.Images.MyEntity;
                    }
                    else
                    {
                        switch (entity.Value.Type)
                        {
                            case EntityTypes.Player:
                                if (player.IsInMyParty(entity.Key))
                                {
                                    color = PluginSettings.Settings.Colors.PartyMember;
                                    texture = PluginSettings.Settings.Images.PartyMember;
                                }
                                else
                                {
                                    color = PluginSettings.Settings.Colors.Player;
                                    texture = PluginSettings.Settings.Images.Player;
                                }
                                break;

                                case EntityTypes.Event:
                                color = PluginSettings.Settings.Colors.Event;
                                texture = PluginSettings.Settings.Images.Event;
                                break;

                            case EntityTypes.GlobalEntity:
                                color = PluginSettings.Settings.Colors.Npc;
                                texture = PluginSettings.Settings.Images.Npc;
                                break;

                            case EntityTypes.Resource:
                                // Okay, this one is a little less straightforward since it relies on users configuring it PROPERLY.
                                // Let's make sure they don't stupid it up and crash the plugin.

                                // Get the tool type and assume we fail at setting our texture and color.
                                var tool = (entity.Value as IResource).BaseResource.Tool;
                                var texSet = false;
                                var colSet = false;

                                // Is the tool a valid one to get the string version for?
                                if (tool >= 0 && tool < mContext.Options.EquipmentOpts.ToolTypes.Count)
                                {
                                    // Get the actual tool type from the server configuration.
                                    var toolType = mContext.Options.EquipmentOpts.ToolTypes[tool];

                                    // Attempt to get our color from the plugin configuration.
                                    if (PluginSettings.Settings.Colors.Resource.TryGetValue(toolType, out color))
                                    {
                                        colSet = true;
                                    }

                                    // Attempt to get our texture from the plugin configuration.
                                    if (PluginSettings.Settings.Images.Resource.TryGetValue(toolType, out texture))
                                    {
                                        texSet = true;
                                    }
                                }
                                // Is it a None tool?
                                else if (tool == -1)
                                {
                                    color = PluginSettings.Settings.Colors.Resource["None"];
                                    colSet = true;
                                    texture = PluginSettings.Settings.Images.Resource["None"];
                                    texSet = true;
                                }

                                // Have we managed to set our color? If not, set to default.
                                if (!colSet)
                                { 
                                    color = PluginSettings.Settings.Colors.Default;
                                }

                                // Have we managed to set our texture? If not, set to blank.
                                if (!texSet)
                                {
                                    texture = string.Empty;
                                }
                                break;

                            case EntityTypes.Projectile:
                                continue;

                            default:
                                color = PluginSettings.Settings.Colors.Default;
                                texture = PluginSettings.Settings.Images.Default;
                                break;
                        }
                    }

                    // Add this to our location dictionary!
                    if (!dict.ContainsKey(map))
                    {
                        dict.Add(map, new Dictionary<Point, EntityInfo>());
                    }

                    // If we already know an entity on this tile.. ignore this!
                    var location = new Point(entity.Value.X, entity.Value.Y);
                    if (!dict[map].ContainsKey(location))
                    {
                        // If not, add our entity!
                        dict[map].Add(location, new EntityInfo() { Color = color, Texture = texture });
                    }
                }
            }

            return dict;
        }

        private GameRenderTexture GenerateBaseRenderTexture()
        {
            var sizeX = (mMinimapTileSize.X * mContext.Options.MapOpts.Width) * 3;
            var sizeY = (mMinimapTileSize.Y * mContext.Options.MapOpts.Height) * 3;

            return mContext.Graphics.CreateRenderTexture(sizeX, sizeY);
        }

        private GameRenderTexture GenerateMapRenderTexture()
        {
            var sizeX = mMinimapTileSize.X * mContext.Options.MapOpts.Width;
            var sizeY = mMinimapTileSize.Y * mContext.Options.MapOpts.Height;

            return mContext.Graphics.CreateRenderTexture(sizeX, sizeY);
        }

        private void GenerateControls()
        {
            // Generate our window control and do not allow users to resize it.
            mWindowControl = mContext.Lifecycle.Interface.Create<WindowControl>(string.Empty, false, mContext.Assembly.FullName);

            // Create our imagepanel and its overlay for the minimap.
            mMinimap = new ImagePanel(mWindowControl, "MinimapContainer");
            mOverlay = new ImagePanel(mWindowControl, "MinimapOverlay");

            mZoomInButton = new Button(mOverlay, "ZoomInButton");
            mZoomInButton.Clicked += MZoomInButton_Clicked;
            mZoomOutButton = new Button(mOverlay, "ZoomOutButton");
            mZoomOutButton.Clicked += MZoomOutButton_Clicked;
        }

        private void MZoomOutButton_Clicked(Base sender, Intersect.Client.Framework.Gwen.Control.EventArguments.ClickedEventArgs arguments)
        {
            if (mZoomLevel < PluginSettings.Settings.MaximumZoom)
            {
                mZoomLevel += PluginSettings.Settings.ZoomStep;
            }
        }

        private void MZoomInButton_Clicked(Base sender, Intersect.Client.Framework.Gwen.Control.EventArguments.ClickedEventArgs arguments)
        {
            if (mZoomLevel > PluginSettings.Settings.MinimumZoom)
            {
                mZoomLevel -= PluginSettings.Settings.ZoomStep;
            }
        }

        private enum MapPosition
        {
            TopLeft,

            TopMiddle,

            TopRight,

            MiddleLeft,

            Middle,

            MiddleRight,

            BottomLeft,

            BottomMiddle,

            BottomRight
        }

        private class EntityInfo
        {
            public Color Color { get; set; }

            public string Texture { get; set; }
        }

    }
}
