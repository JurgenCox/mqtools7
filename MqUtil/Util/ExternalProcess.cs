using MqApi.Util;
namespace MqUtil.Util{
	public abstract class ExternalProcess{
		public void Run(string[] args, bool debug){
			DateTime start = DateTime.Now;
			if (debug){
				MqProcessInfo.StartLog(args[0], args[3],
					string.IsNullOrEmpty(args[4]) ? StringUtils.Concat(" ", args) : args[4], start);
				Function(args, new Responder(args[0], args[3]));
				MqProcessInfo.EndLog(args[0], args[3],
					string.IsNullOrEmpty(args[4]) ? StringUtils.Concat(" ", args) : args[4], start, DateTime.Now);
			} else{
				try{
					MqProcessInfo.StartLog(args[0], args[3],
						string.IsNullOrEmpty(args[4]) ? StringUtils.Concat(" ", args) : args[4], start);
					Function(args, new Responder(args[0], args[3]));
					MqProcessInfo.EndLog(args[0], args[3],
						string.IsNullOrEmpty(args[4]) ? StringUtils.Concat(" ", args) : args[4], start, DateTime.Now);
				} catch (Exception e){
					MqProcessInfo.ErrorLog(args[0], args[3],
						string.IsNullOrEmpty(args[4]) ? StringUtils.Concat(" ", args) : args[4], start, DateTime.Now,
						e.Message + "_" + StringUtils.Replace(e.StackTrace, new[]{"\r", "\n"}, "_"));
					throw;
				}
			}
		}

		protected abstract void Function(string[] args, Responder responder);
	}
}