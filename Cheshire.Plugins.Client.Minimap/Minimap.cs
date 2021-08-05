﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Cheshire.Plugins.Utilities.Client.Interface;
using Cheshire.Plugins.Client.WebButtons.Configuration;

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

namespace Cheshire.Plugins.Client.Minimap
{
    public class Minimap
    { 
        private IClientPluginContext mContext;

        private Point mMinimapTileSize;

        private string mPluginDir;

        private GameRenderTexture mRenderTexture;

        private ImagePanel mMinimap;

        private WindowControl mWindowControl;

        private bool mRedrawMaps;

        private bool mRedrawEntities;

        private Dictionary<MapPosition, GameRenderTexture> mMinimapCache = new Dictionary<MapPosition, GameRenderTexture>();
        
        private Dictionary<MapPosition, GameRenderTexture> mEntityCache = new Dictionary<MapPosition, GameRenderTexture>();

        private Dictionary<MapPosition, Dictionary<Point, Color>> mEntityInfoCache = new Dictionary<MapPosition, Dictionary<Point, Color>>();

        private Dictionary<MapPosition, MapBase> mMapGrid = new Dictionary<MapPosition, MapBase>();

        private Dictionary<Guid, MapPosition> mMapPosition = new Dictionary<Guid, MapPosition>();

        private GameRenderTexture mWhiteTexture;

        public Minimap(IClientPluginContext context, int tileSizeX, int tileSizeY, string pluginDir)
        {
            mContext = context;
            mMinimapTileSize = new Point(tileSizeX, tileSizeY);
            mPluginDir = pluginDir;
        }

        public void Initialize()
        {
            // Load our graphical assets into the engine!
            mContext.ContentManager.LoadAssets(Path.Combine(mPluginDir, "resources"), new List<ContentTypes>() { ContentTypes.Interface });

            // Generate our GUI controls and load their layout.
            GenerateControls();
            mWindowControl.LoadJsonUI(Path.Combine(mPluginDir, "resources", "gui", "layouts", "game", "MinimapLayout.json"));

            // Generate some textures that we'll be using for rendering..
            mWhiteTexture = mContext.Graphics.GetWhiteTexture() as GameRenderTexture;
            mRenderTexture = GenerateBaseRenderTexture();

            // Set our minimap background texture.
            mMinimap.Texture = mRenderTexture;
            mMinimap.SetTextureRect(0, 0, mRenderTexture.Width, mRenderTexture.Height);
        }

        public void Update(IEntity entity, IReadOnlyDictionary<Guid, IEntity> allEntities)
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
                foreach (var layer in mContext.Options.MapOpts.Layers.LowerLayers)
                {
                    for (var x = 0; x < mContext.Options.MapOpts.Width; x++)
                    {
                        for (var y = 0; y < mContext.Options.MapOpts.Height; y++)
                        {
                            var curTile = mMapGrid[position].Layers[layer][x, y];
                            if (curTile.TilesetId != Guid.Empty)
                            {
                                var tileset = TilesetBase.Get(curTile.TilesetId);
                                var texture = mContext.ContentManager.Find<GameTexture>(Intersect.Client.Framework.Content.ContentTypes.TileSet, tileset.Name);

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
                    mContext.Graphics.DrawTexture(
                        mWhiteTexture,
                        0,
                        0,
                        1,
                        1,
                        entity.Key.X * mMinimapTileSize.X,
                        entity.Key.Y * mMinimapTileSize.Y,
                        mMinimapTileSize.X,
                        mMinimapTileSize.Y,
                        entity.Value,
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

        private Dictionary<MapPosition, Dictionary<Point, Color>> GenerateEntityInfo(IReadOnlyDictionary<Guid, IEntity> entities, IEntity myEntity)
        {
            var dict = new Dictionary<MapPosition, Dictionary<Point, Color>>();
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

                    // Get a render colour for this entity.. Force our own to be different!
                    // Force our own to be a different colour!
                    if (entity.Key == myEntity.Id)
                    {
                        color = PluginSettings.Settings.Colors.MyEntity;
                    }
                    else
                    {
                        switch (entity.Value.Type)
                        {
                            case EntityTypes.Player:
                                color = PluginSettings.Settings.Colors.Player;
                                break;

                                case EntityTypes.Event:
                                color = PluginSettings.Settings.Colors.Event;
                                break;

                            case EntityTypes.GlobalEntity:
                                color = PluginSettings.Settings.Colors.Npc;
                                break;

                            case EntityTypes.Resource:
                                color = PluginSettings.Settings.Colors.Resource;
                                break;

                            case EntityTypes.Projectile:
                                continue;

                            default:
                                color = PluginSettings.Settings.Colors.Default;
                                break;
                        }
                    }

                    // Add this to our location dictionary!
                    if (!dict.ContainsKey(map))
                    {
                        dict.Add(map, new Dictionary<Point, Color>());
                    }

                    // If we already know an entity on this tile.. ignore this!
                    var location = new Point(entity.Value.X, entity.Value.Y);
                    if (!dict[map].ContainsKey(location))
                    {
                        // If not, add our entity!
                        dict[map].Add(location, color);
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
            mWindowControl.DisableResizing();

            // Create our imagepanel and its overlay for the minimap.
            mMinimap = new ImagePanel(mWindowControl, "MinimapContainer");
            var overlay = new ImagePanel(mWindowControl, "MinimapOverlay");
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

    }
}
