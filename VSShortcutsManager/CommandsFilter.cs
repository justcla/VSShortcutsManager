using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.PatternMatching;
using System;
using System.Globalization;
using System.Linq;

namespace VSShortcutsManager
{
    interface ICommandsFilter
    {
        VSCommandShortcuts Filter(VSCommandShortcuts commands);
    }

    class ExactMatchCommandsFilter : ICommandsFilter
    {
        public ExactMatchCommandsFilter(string searchCriteria, bool matchCase)
        {
            this.searchCriteria = searchCriteria;
            this.stringComparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        }

        public VSCommandShortcuts Filter(VSCommandShortcuts commands)
        {
            var result = commands
                .Where(command => command.Command.IndexOf(this.searchCriteria, this.stringComparison) >= 0);

            return new VSCommandShortcuts(result);
        }

        private readonly string searchCriteria;
        private readonly StringComparison stringComparison;
    }

    class FuzzyMatchCommandsFilter : ICommandsFilter
    {
        public FuzzyMatchCommandsFilter(IPatternMatcher patternMatcher)
        {
            this.patternMatcher = patternMatcher;
        }

        public VSCommandShortcuts Filter(VSCommandShortcuts commands)
        {
            var result = commands
                .Select(command => (command: command, match: patternMatcher.TryMatch(command.Command)))
                .Where(tuple => tuple.match != null)
                .OrderBy(tuple => tuple.match)
                .Select(tuple => tuple.command);

            return new VSCommandShortcuts(result);
        }

        private readonly IPatternMatcher patternMatcher;
    }

    class CommandsFilterFactory
    {
        public ICommandsFilter GetCommandsFilter(string searchCriteria, bool matchCase)
        {
            if (matchCase)
            {
                return new ExactMatchCommandsFilter(searchCriteria, matchCase: true);
            }

            var componentModel = VSShortcutsManagerPackage.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            if (componentModel == null)
            {
                return new ExactMatchCommandsFilter(searchCriteria, matchCase: false);
            }

            var patternMatcherFactory = componentModel.GetService<IPatternMatcherFactory>();
            if (patternMatcherFactory == null)
            {
                return new ExactMatchCommandsFilter(searchCriteria, matchCase: false);
            }

            var patternMatcher = patternMatcherFactory.CreatePatternMatcher(
                searchCriteria,
                new PatternMatcherCreationOptions(
                    CultureInfo.CurrentCulture,
                    PatternMatcherCreationFlags.AllowFuzzyMatching));

            return new FuzzyMatchCommandsFilter(patternMatcher);
        }
    }
}
