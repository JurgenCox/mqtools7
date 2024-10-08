using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Search {
    public class CrosslinkSite {
        public CrossLinkType Type;
        public byte Site1;
        public byte Site2;

        public CrosslinkSite(CrossLinkType type, byte site1, byte site2) {
            Type = type;
            Site1 = site1;
            Site2 = site2;
        }

        protected bool Equals(CrosslinkSite other) {
            return Type == other.Type && Site1 == other.Site1 && Site2 == other.Site2;
        }

        public override string ToString() {
            return base.ToString();
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CrosslinkSite) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Site1;
                hashCode = (hashCode * 397) ^ Site2;
                return hashCode;
            }
        }
    }
}