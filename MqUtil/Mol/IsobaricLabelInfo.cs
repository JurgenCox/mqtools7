﻿using MqUtil.Util;
namespace MqUtil.Mol {
	public abstract class IsobaricLabelInfo {
		public List<InputParameter> vals;
		public readonly Dictionary<string, InputParameter> map;
		public string internalLabel;
		public string terminalLabel;
		public bool tmtLike;
		protected IsobaricLabelInfo(string internalLabel, string terminalLabel, bool tmtLike) {
			map = new Dictionary<string, InputParameter>();
			this.internalLabel = internalLabel;
			this.terminalLabel = terminalLabel;
			this.tmtLike = tmtLike;
		}

	}
}
