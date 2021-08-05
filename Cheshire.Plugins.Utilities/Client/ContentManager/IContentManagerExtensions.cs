using System;
using System.IO;
using System.Collections.Generic;

using Intersect.Client.Framework.Audio;
using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.Graphics;

namespace Cheshire.Plugins.Utilities.Client.ContentManager
{
    public static class IContentManagerExtensions
    {
        /// <summary>
        /// Load all assets from a root directory, much like the base engine would from its own resources directory.
        /// </summary>
        /// <param name="manager">The content manager instance to use for loading assets with.</param>
        /// <param name="rootPath">The root resources directory to search for our files in.</param>
        /// <param name="contentTypes"></param>
        public static void LoadAssets(this IContentManager manager, string rootPath, List<ContentTypes> contentTypes)
        {
            foreach(var type in contentTypes)
            {
                var path = string.Empty;
                var extension = string.Empty;
                var isTexture = true;

                switch (type)
                {
                    case ContentTypes.Animation:
                        path = "animations";
                        extension = "*.png";
                        break;
                    case ContentTypes.Entity:
                        path = "entities";
                        extension = "*.png";
                        break;
                    case ContentTypes.Face:
                        path = "faces";
                        extension = "*.png";
                        break;
                    case ContentTypes.Fog:
                        path = "fogs";
                        extension = "*.png";
                        break;
                    case ContentTypes.Image:
                        path = "images";
                        extension = "*.png";
                        break;
                    case ContentTypes.Interface:
                        path = "gui";
                        extension = "*.png";
                        break;
                    case ContentTypes.Item:
                        path = "items";
                        extension = "*.png";
                        break;
                    case ContentTypes.Miscellaneous:
                        path = "misc";
                        extension = "*.png";
                        break;
                    case ContentTypes.Music:
                        path = "music";
                        extension = "*.ogg";
                        isTexture = false;
                        break;
                    case ContentTypes.Paperdoll:
                        path = "paperdolls";
                        extension = "*.png";
                        break;
                    case ContentTypes.Resource:
                        path = "resources";
                        extension = "*.png";
                        break;
                    case ContentTypes.Sound:
                        path = "sounds";
                        extension = "*.wav";
                        isTexture = false;
                        break;
                    case ContentTypes.Spell:
                        path = "spells";
                        extension = "*.png";
                        break;
                    case ContentTypes.TileSet:
                        path = "tilesets";
                        extension = "*.png";
                        break;
                    case ContentTypes.Font:
                    case ContentTypes.Shader:
                    case ContentTypes.TexturePack:
                        throw new NotImplementedException();

                    default:
                        throw new NotImplementedException();
                }

                var searchPath = Path.Combine(rootPath, path);
                if (!Directory.Exists(searchPath))
                {
                    Directory.CreateDirectory(searchPath);
                }

                foreach (var file in Directory.EnumerateFiles(searchPath, extension))
                {
                    if (isTexture)
                    {
                        LoadTexture(manager, type, file);
                    }
                    else
                    {
                        LoadAudio(manager, type, file);
                    }
                }
            }
        }

        private static void LoadTexture(IContentManager manager, ContentTypes type, string path)
        {
            manager.Load<GameTexture>(type, path, Path.GetFileName(path));
        }

        private static void LoadAudio(IContentManager manager, ContentTypes type, string path)
        {
            manager.Load<GameAudioSource>(type, path, Path.GetFileName(path));
        }
    }
}
