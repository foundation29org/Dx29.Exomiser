using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Dx29.Exomiser
{
    static public class ExomiserAnalysisExtensions
    {
        static public string AsYAML(this ExomiserAnalysis exomiserAnalysis)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new DecimalYamlTypeConverter())
                .JsonCompatible()
                .Build();

            return serializer.Serialize(exomiserAnalysis);
        }
    }

    sealed class DecimalYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(decimal);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new Scalar(null, $"{value:0.00}"));
        }
    }
}
