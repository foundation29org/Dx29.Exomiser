using System;
using System.Collections.Generic;

namespace Dx29.Exomiser
{
    static public class ExomiserRequestExtensions
    {
        static public ExomiserAnalysis AsExomiserAnalysis(this ExomiserRequest exomiserRequest)
        {
            var exomiser = ExomiserAnalysis.CreateDefault();
            var analysis = exomiser.analysis;

            analysis.genomeAssembly = exomiserRequest.GenomeAssembly;

            analysis.vcf = null;
            analysis.ped = null;
            analysis.proband = exomiserRequest.Proband;

            analysis.hpoIds = exomiserRequest.Hpos ?? new string[] { };
            analysis.analysisMode = exomiserRequest.AnalysisMode;
            analysis.frequencySources = exomiserRequest.FrequencySources;
            analysis.pathogenicitySources = new List<string>(exomiserRequest.PathogenicitySources);
            if (exomiserRequest.IsGenome)
                analysis.pathogenicitySources.Add("REMM");

            var inheritanceModes = analysis.inheritanceModes;
            inheritanceModes.AUTOSOMAL_DOMINANT = (decimal)exomiserRequest.InheritanceModes["AUTOSOMAL_DOMINANT"];
            inheritanceModes.AUTOSOMAL_RECESSIVE_HOM_ALT = (decimal)exomiserRequest.InheritanceModes["AUTOSOMAL_RECESSIVE_HOM_ALT"];
            inheritanceModes.AUTOSOMAL_RECESSIVE_COMP_HET = (decimal)exomiserRequest.InheritanceModes["AUTOSOMAL_RECESSIVE_COMP_HET"];
            inheritanceModes.X_DOMINANT = (decimal)exomiserRequest.InheritanceModes["X_DOMINANT"];
            inheritanceModes.X_RECESSIVE_HOM_ALT = (decimal)exomiserRequest.InheritanceModes["X_RECESSIVE_HOM_ALT"];
            inheritanceModes.X_RECESSIVE_COMP_HET = (decimal)exomiserRequest.InheritanceModes["X_RECESSIVE_COMP_HET"];
            inheritanceModes.MITOCHONDRIAL = (decimal)exomiserRequest.InheritanceModes["MITOCHONDRIAL"];

            var steps = analysis.steps;
            steps.Add(new { qualityFilter = new QualityFilter { minQuality = (decimal)exomiserRequest.MinQuality } });
            if (exomiserRequest.VariantEffectFilters.ContainsKey("remove"))
                steps.Add(new { variantEffectFilter = new VariantEffectFilter { remove = exomiserRequest.VariantEffectFilters["remove"] } });
            steps.Add(new { frequencyFilter = new FrequencyFilter { maxFrequency = (decimal)exomiserRequest.Frequency } });
            steps.Add(new { pathogenicityFilter = new PathogenicityFilter { keepNonPathogenic = exomiserRequest.KeepNonPathogenic } });
            steps.Add(new { inheritanceFilter = new InheritanceFilter() });
            steps.Add(new { omimPrioritiser = new OmimPrioritiser() });
            if (exomiserRequest.PriorityScoreFilter != null)
            {
                steps.Add(new { priorityScoreFilter = exomiserRequest.PriorityScoreFilter });
            }
            if (analysis.hpoIds.Count > 0)
            {
                steps.Add(new { hiPhivePrioritiser = new HiPhivePrioritiser { runParams = String.Join(", ", exomiserRequest.HiPhivePrioritisers) } });
            }
            if (exomiserRequest.RegulatoryFeatureFilter)
            {
                steps.Add(new { regulatoryFeatureFilter = new RegulatoryFeatureFilter() });
            }

            var output = exomiser.outputOptions;
            output.numGenes = exomiserRequest.NumGenes;
            output.outputFormats = exomiserRequest.OutputFormats;
            output.outputContributingVariantsOnly = exomiserRequest.OutputPassVariantsOnly;

            return exomiser;
        }
    }
}
