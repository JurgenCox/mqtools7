using MqUtil.Util;
namespace MqUtil.Base {
	public class FragmentationTypeSettings {
		public readonly List<InputParameter> vals = new List<InputParameter> {
			new InputParameter<string>("Name", "Name"),
			new InputParameter<bool>("UseIntensityPrediction", "UseIntensityPrediction"),
            new InputParameter<bool>("UseSequenceBasedModifier", "UseSequenceBasedModifier"),
            new InputParameter<bool>("InternalFragments", "InternalFragments"),
			new InputParameter<double>("InternalFragmentWeight", "InternalFragmentWeight"),
			new InputParameter<string>("InternalFragmentAas", "InternalFragmentAas")
		};

		public readonly Dictionary<string, InputParameter> map;

		public FragmentationTypeSettings() {
			map = new Dictionary<string, InputParameter>();
			foreach (InputParameter val in vals) {
				map.Add(val.Name, val);
			}
		}

		public FragmentationTypeSettings(string name, bool useIntensityPrediction, bool useSequenceBasedModifier, bool internalFragments, double internalFragmentWeight, string internalFragmentAas) {
			Name = name;
			UseIntensityPrediction = useIntensityPrediction;
			UseSequenceBasedModifier = useSequenceBasedModifier;
            InternalFragments = internalFragments;
			InternalFragmentWeight = internalFragmentWeight;
			InternalFragmentAas = internalFragmentAas;
		}

		public FragmentationTypeSettings Clone() {
			return new FragmentationTypeSettings(Name, UseIntensityPrediction, UseSequenceBasedModifier, InternalFragments, InternalFragmentWeight, InternalFragmentAas);
		}

		public string Name { get; set; }
		public bool UseIntensityPrediction { get; set; }
        public bool UseSequenceBasedModifier { get; set; }
        public bool InternalFragments { get; set; }
		public double InternalFragmentWeight { get; set; }
		public string InternalFragmentAas { get; set; }
	}
}