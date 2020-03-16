﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using SparkleXrm.Tasks.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SparkleXrm.Tasks {
    public class DeployPluginsAndWorkflowTask : BaseTask
    {
        public bool ExcludePluginSteps { get; set; }

        public DeployPluginsAndWorkflowTask(IOrganizationService service, ITrace trace) : base(service, trace)
        {
          
        }
        public DeployPluginsAndWorkflowTask(OrganizationServiceContext ctx, ITrace trace) : base(ctx, trace)
        {

        }

        protected override void ExecuteInternal(string folder, OrganizationServiceContext ctx)
        {
            _trace.WriteLine("Searching for plugin and workflow config in '{0}'", folder);
            var configs = ServiceLocator.ConfigFileFactory.FindConfig(folder);

            foreach (var config in configs)
            {
                _trace.WriteLine("Using Config '{0}'", config.filePath);
                DeployPluginsAndWorkflows(ctx, config);
            }
            _trace.WriteLine("Processed {0} config(s)", configs.Count);
        }

        private void DeployPluginsAndWorkflows(OrganizationServiceContext ctx, ConfigFile config)
        {
            var plugins = config.GetPluginsConfig(this.Profile);
            
            foreach (var plugin in plugins)
            {
                List<string> assemblies = config.GetAssemblies(plugin);

                var pluginRegistration = new PluginRegistraton(_service, ctx, _trace);

                if (!string.IsNullOrEmpty(plugin.solution))
                {
                    pluginRegistration.SolutionUniqueName = plugin.solution;
                }

                foreach (var assemblyFilePath in assemblies)
                {
                    try
                    {
                        pluginRegistration.RegisterPluginAndWorkflow(assemblyFilePath, ExcludePluginSteps);
                    }

                    catch (ReflectionTypeLoadException ex)
                    {
                        throw new Exception(ex.LoaderExceptions.First().Message);
                    }
                }
            }

        }
    }
}