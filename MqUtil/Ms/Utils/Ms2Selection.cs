namespace MqUtil.Ms.Utils{
	public class Ms2Selection{
		private bool[] ms2Selection;
		private bool multiSelect;
		private int singleSelectMs2Index;
		public void SelectFirstScan(){
			multiSelect = false;
			ms2Selection = null;
			singleSelectMs2Index = 0;
		}
		public void SelectLastScan(int ms2Count){
			multiSelect = false;
			ms2Selection = null;
			singleSelectMs2Index = ms2Count - 1;
		}
		public void SelectNextScan(int ms2Count){
			if (!Any()){
				SelectFirstScan();
				return;
			}
			int selected = GetSelection()[0];
			int ms2Ind = selected;
			multiSelect = false;
			ms2Selection = null;
			singleSelectMs2Index = Math.Min(ms2Ind + 1, ms2Count - 1);
		}
		public void SelectPreviousScan(){
			if (!Any()){
				SelectFirstScan();
				return;
			}
			int selected = GetSelection()[0];
			int ms2Ind = selected;
			multiSelect = false;
			ms2Selection = null;
			singleSelectMs2Index = Math.Max(ms2Ind - 1, 0);
		}
		public bool Select(List<int> indices, bool add, bool toggle, int ms2Count){
			if (indices == null || !add && !toggle && indices.Count <= 1){
				if (!multiSelect && indices != null && indices.Count > 0){
					if (singleSelectMs2Index == indices[0]){
						return false;
					}
				}
				multiSelect = false;
				ms2Selection = null;
				if (indices == null || indices.Count == 0){
					singleSelectMs2Index = -1;
					return true;
				}
				singleSelectMs2Index = indices[0];
				return true;
			}
			multiSelect = true;
			bool isNew = EnsureSelection(ms2Count);
			if (!add && !isNew){
				ClearSelectionMulti();
			}
			foreach (int p in indices){
				int ind = p;
				if (ind == -1){
					continue;
				}
				if (toggle){
					ms2Selection[ind] = !ms2Selection[ind];
				} else{
					ms2Selection[ind] = true;
				}
			}
			return true;
		}
		public List<int> GetSelection(){
			if (!multiSelect){
				return singleSelectMs2Index < 0 ? new List<int>() : new List<int>{singleSelectMs2Index};
			}
			List<int> result = new List<int>();
			if (ms2Selection == null){
				return result;
			}
			for (int i = 0; i < ms2Selection.Length; i++){
				if (ms2Selection[i]){
					result.Add(i);
				}
			}
			return result;
		}
		public void ClearSelection(){
			multiSelect = false;
			ms2Selection = null;
			singleSelectMs2Index = -1;
		}
		private void ClearSelectionMulti(){
			for (int i = 0; i < ms2Selection.Length; i++){
				ms2Selection[i] = false;
			}
		}
		private bool EnsureSelection(int ms2Count){
			if (ms2Selection == null){
				ms2Selection = new bool[ms2Count];
				return true;
			}
			return false;
		}
		public bool Any(){
			if (multiSelect){
				foreach (bool b in ms2Selection){
					if (b){
						return true;
					}
				}
				return false;
			}
			return singleSelectMs2Index >= 0;
		}
		public int GetSelectedIndex(){
			List<int> s = GetSelection();
			if (s.Count != 1){
				return -1;
			}
			return s[0];
		}
	}
}