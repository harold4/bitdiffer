using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Policy;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;

using BitDiffer.Common.Model;
using BitDiffer.Extractor;
using BitDiffer.Common.Utility;
using BitDiffer.Common.Misc;
using BitDiffer.Common.Configuration;

namespace BitDiffer.Core
{
    public abstract class AssemblyManager
    {
        private static int _domainID = 1;
        private static string _extractorTypeName;

        static AssemblyManager()
        {
            _extractorTypeName = new AssemblyExtractor().GetType().FullName; // Do it this way to work with obfuscator
        }

        public AssemblyManager()
        {
        }

        public AssemblyDetail ExtractAssemblyInf(string assemblyPath, DiffConfig config)
        {
            if (!Path.IsPathRooted(assemblyPath))
            {
                assemblyPath = Path.GetFullPath(assemblyPath);
            }

            Log.Verbose("Extracting from assembly {0}", Path.GetFileName(assemblyPath));

            var deleteFiles = new ConcurrentQueue<string>();
            DomainExtractorPair pair = GetExtractor(assemblyPath);
            AssemblyDetail ad = pair.Extractor.ExtractFrom(assemblyPath, config, ref deleteFiles);
            OneExtractionComplete(pair);
            OnUnloadDeleteFiles(deleteFiles);
            return ad;
        }

        protected abstract DomainExtractorPair GetExtractor(string path);

        protected virtual void OneExtractionComplete(DomainExtractorPair pair)
        {
        }

        internal virtual void AllExtractionsComplete()
        {
        }

        protected DomainExtractorPair GetExtractorInTempAppDomain(string assemblyPath)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyPath);
            setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            Evidence evidence = new Evidence(AppDomain.CurrentDomain.Evidence);

            Interlocked.Increment(ref _domainID);

            string appDomainName = Constants.ExtractionDomainPrefix + " " + _domainID.ToString();
            string typeName = new AssemblyExtractor().GetType().FullName; // Do it this way to work with obfuscator
            string extractorPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BitDiffer.Extractor.dll");

            Log.Verbose("Creating {0}", appDomainName);
            AppDomain domain = AppDomain.CreateDomain(appDomainName, evidence, setup);
            AssemblyExtractor extractor = (AssemblyExtractor)domain.CreateInstanceFromAndUnwrap(extractorPath, _extractorTypeName);

            // When running in another app domain - need to copy the Visual Studio trace listeners over.
            // This allows unit tests running in the other AppDomain to have their trace output displayed in the Visual Studio trace output.
            // TraceListener is MarshalByRef so this is safe.
            foreach (TraceListener listener in Trace.Listeners)
            {
                if (listener.Name == "")
                {
                    extractor.AddTraceListener(listener);
                }
            }

            return new DomainExtractorPair(domain, extractor);
        }

        // private static ConcurrentQueue<string> _deleteFiles = new ConcurrentQueue<string>();

        // public static void AddDeleteFileOnUnload(string filePath)
        // {
        //     _deleteFiles.Enqueue(filePath);
        // }
        private void OnUnloadDeleteFiles(ConcurrentQueue<string> deleteFiles)
        {
            while (deleteFiles.TryDequeue(out string deleteFile))
            {
                try
                {
                    File.Delete(deleteFile);
                    Log.Info("Deleted "+deleteFile);
                }
                catch (Exception e)
                {
                    Log.Warn($"Delete failed: {deleteFile}. {e.Message}");
                }
            }
        }
    }
}
