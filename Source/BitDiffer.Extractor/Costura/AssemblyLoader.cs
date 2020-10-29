using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using BitDiffer.Common.Utility;

namespace BitDiffer.Extractor.Costura
{
    internal class AssemblyLoader
    {
        private readonly ConcurrentQueue<string> _deleteFileList;
        private Assembly _executingAssembly;

        private List<Item> _items;

        public AssemblyLoader(Assembly executingAssembly, ConcurrentQueue<string> deleteFileList)
        {
            _executingAssembly = executingAssembly;
            _deleteFileList = deleteFileList;
            var items = new List<Item>();
            var names = _executingAssembly.GetManifestResourceNames();
            foreach (var fullname in names)
            {
                if (fullname.StartsWith("costura.", StringComparison.Ordinal) && fullname.EndsWith(".compressed", StringComparison.Ordinal))
                {
                    var name = fullname.Substring(8, fullname.Length - 8 - 11);
                    if (!name.EndsWith(".dll", StringComparison.Ordinal)) continue;

                    name = name.Substring(0, name.Length - 4);
                    items.Add(new Item(this, name, fullname));
                }
            }
            _items = items;
        }

        public void CreateDlls(string assemblyDirectory)
        {
            foreach (var item in _items)
            {
                item.Save(assemblyDirectory);
            }
        }

        private void CopyTo(Stream source, Stream destination)
        {
            byte[] array = new byte[81920];
            int count;
            while ((count = source.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }

        private Stream LoadStream(string fullName)
        {
            Assembly executingAssembly = _executingAssembly;
            if (fullName.StartsWith("costura.", StringComparison.Ordinal) && fullName.EndsWith(".compressed", StringComparison.Ordinal))
            {
                using (Stream stream = executingAssembly.GetManifestResourceStream(fullName))
                {
                    using (DeflateStream source = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        CopyTo(source, memoryStream);
                        memoryStream.Position = 0L;
                        return memoryStream;
                    }
                }
            }
            return executingAssembly.GetManifestResourceStream(fullName);
        }

        private byte[] ReadStream(Stream stream)
        {
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            return array;
        }

        // private Assembly ReadFromEmbeddedResources(Dictionary<string, string> assemblyNames, Dictionary<string, string> symbolNames,
        //     AssemblyName requestedAssemblyName)
        // {
        //     Log.Info($"   ReadFromEmbeddedResources: {requestedAssemblyName}");
        //     string text = requestedAssemblyName.Name.ToLowerInvariant();
        //     if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(requestedAssemblyName.CultureInfo.Name))
        //     {
        //         text = requestedAssemblyName.CultureInfo.Name + "." + text;
        //     }
        //     byte[] rawAssembly;
        //     using (Stream stream = LoadStream(assemblyNames, text))
        //     {
        //         if (stream == null)
        //         {
        //             return null;
        //         }
        //         rawAssembly = ReadStream(stream);
        //     }
        //     using (Stream stream2 = LoadStream(symbolNames, text))
        //     {
        //         Log.Info($"   ReadFromEmbeddedResources: {text}: {stream2}");
        //         if (stream2 != null)
        //         {
        //             byte[] rawSymbolStore = ReadStream(stream2);
        //             return _reflectionOnly ? Assembly.ReflectionOnlyLoad(rawAssembly) : Assembly.Load(rawAssembly, rawSymbolStore);
        //         }
        //     }
        //     return _reflectionOnly ? Assembly.ReflectionOnlyLoad(rawAssembly) : Assembly.Load(rawAssembly);
        // }


        class Item
        {
            private readonly AssemblyLoader _assemblyLoader;

            public Item(AssemblyLoader assemblyLoader, string assemblyName, string resourceName)
            {
                _assemblyLoader = assemblyLoader;
                AssemblyName = assemblyName;
                ResourceName = resourceName;
            }

            public string AssemblyName { get; }
            public string ResourceName { get; }

            public void Save(string directory)
            {
                using (var stream = _assemblyLoader.LoadStream(ResourceName))
                {
                    if (stream == null)
                    {
                        Log.Error($"Resource not found!?? {ResourceName}");
                        return;
                    }
                    var rawStream = _assemblyLoader.ReadStream(stream);
                    var dllName = Path.Combine(directory, AssemblyName + ".dll");
                    if (!File.Exists(dllName))
                    {
                        File.WriteAllBytes(dllName, rawStream);
                        _assemblyLoader._deleteFileList.Enqueue(dllName);
                    }
                    Log.Verbose($"Assembly temporary saved: {AssemblyName}");
                }
            }
        }
    }
}