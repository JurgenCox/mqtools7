using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
using MqApi.Util;
using MqUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginInterop.R{
	public class MatrixProcessing : PluginInterop.MatrixProcessing{
		public override string Name => "R";
		public override string Description => "Run R script";
		public override Bitmap2 DisplayImage => Bitmap2.GetImage("Rlogo.png");
		protected override string CodeFilter => "R script, *.R | *.R";
		protected override FileParam ExecutableParam(){
			return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable);
		}
		protected override bool TryFindExecutable(out string path){
			return Utils.GetRScriptPath(out path);
		}
		public override EditorType Edit => EditorType.CodeR;


		// Gets the user's R code and turns it into a temporary runtime wrapper script.
		//
		// Internal mode stores code directly in the parameter.
		// External mode stores a path to an .R file, so we read that file first.
		//
		// We return the generated wrapper path instead of the original user script path.
		// The wrapper provides df, runs the user's code, checks result_df, and writes the Perseus output.
		protected override bool TryGetCodeFile(Parameters param, out string codeFile, out ScriptMode scriptMode)
		{
			codeFile = string.Empty;
			scriptMode = ScriptMode.Internal;

			ParameterWithSubParams<int> scriptModeParam = param.GetParamWithSubParams<int>("Script mode");
			scriptMode = (ScriptMode)scriptModeParam.Value;
			Parameters paramsModel = scriptModeParam.GetSubParameters();

			string rawUserCode;
			if (scriptMode == ScriptMode.Internal)
			{
				string[] scriptText = paramsModel.GetParam<string[]>("Script text").Value;
				rawUserCode = string.Join(Environment.NewLine, scriptText ?? Array.Empty<string>());
			}
			else
			{
				string rawCodeFile = paramsModel.GetParam<string>(CodeLabel).Value;
				if (string.IsNullOrWhiteSpace(rawCodeFile) || !File.Exists(rawCodeFile))
				{
					return false;
				}

				rawUserCode = File.ReadAllText(rawCodeFile);
			}

			codeFile = BuildWrappedRRuntimeScript(rawUserCode, NumSupplTables);
			return true;
		}

		// Runs R processing using the generated runtime wrapper.
		// The wrapper makes runtime behavior match the inline editor style:
		// user code receives df and must assign result_df.
		public override void ProcessData(
			IMatrixData mdata,
			Parameters param,
			ref IMatrixData[] supplTables,
			ref IDocumentData[] documents,
			ProcessInfo processInfo)
		{
			string remoteExe = param.GetParam<string>(InterpreterLabel).Value;
			if (string.IsNullOrWhiteSpace(remoteExe))
			{
				processInfo.ErrString = remoteExeNotSpecified;
				return;
			}

			string inFile = Path.GetTempFileName();
			PerseusUtils.WriteMatrixToFile(mdata, inFile, AdditionalMatrices);

			string outFile = Path.GetTempFileName();

			if (!TryGetCodeFile(param, out string codeFile, out ScriptMode scriptMode))
			{
				processInfo.ErrString = $"Code file '{codeFile}' was not found";
				return;
			}

			try
			{
				if (supplTables == null)
				{
					supplTables = Enumerable.Range(0, NumSupplTables)
						.Select(i => (IMatrixData)mdata.CreateNewInstance())
						.ToArray();
				}

				string[] suppFiles = supplTables.Select(i => Path.GetTempFileName()).ToArray();
				string commandLineArguments = GetCommandLineArguments(param);
				string args = $"{codeFile} {commandLineArguments} {inFile} {outFile} {string.Join(" ", suppFiles)}";

				Debug.WriteLine($"executing > {remoteExe} {args}");

				if (global::PluginInterop.Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
				{
					processInfo.ErrString = processInfoErrString;
					return;
				}

				mdata.Clear();
				PerseusUtil.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');

				for (int i = 0; i < NumSupplTables; i++)
				{
					PerseusUtil.ReadMatrixFromFile(supplTables[i], processInfo, suppFiles[i], '\t');
				}
			}
			finally
			{
				// Delete only the generated runtime wrapper.
				// Do not delete the user's original external script file.
				try
				{
					if (!string.IsNullOrWhiteSpace(codeFile) &&
						File.Exists(codeFile) &&
						Path.GetFileName(codeFile).StartsWith("perseus_r_runtime_"))
					{
						File.SetAttributes(codeFile, FileAttributes.Normal);
						File.Delete(codeFile);
					}
				}
				catch
				{
					// Best effort cleanup.
				}
			}
		}

		// Builds a temporary R runtime wrapper for the normal Perseus processing node.
		//
		// This wrapper makes old Internal/External script execution work like the inline editor:
		// the user's code receives df and must assign result_df.
		//
		// Important:
		// This runtime wrapper currently reads the exported Perseus text file as a full
		// tab-delimited table, then writes the result back while trying to preserve Perseus
		// metadata rows. I tried using PerseusR::read.perseus here, but it did not preserve
		// the full table shape/annotation grouping correctly in this runtime path.
		//
		// TODO:
		// Revisit this and try to make PerseusR::read.perseus work correctly here too,
		// so the runtime path and inline preview/testing path can use the same reader.
		//
		// TODO:
		// Consider switching this R wrapper to Base64/source-style evaluation like Python.
		// Direct insertion is simpler because R is not indentation-sensitive like Python,
		// but Base64 would make Python/R runtime wrappers more consistent and safer around wrapper boundaries.
		private static string BuildWrappedRRuntimeScript(string rawUserCode, int numSupplTables)
		{
			string escapedUserCode = rawUserCode ?? string.Empty;

			string argumentSetup = $@"
num_suppl_tables <- {numSupplTables}

if (length(args) < 2 + num_suppl_tables) {{
	fail('Not enough arguments. Expected inFile, outFile, and optional supplementary output files.')
}}

inFile <- args[length(args) - num_suppl_tables - 1]
outFile <- args[length(args) - num_suppl_tables]
";

			string script = @"
args <- commandArgs(trailingOnly = TRUE)

fail <- function(msg, code = 1) {
	message(msg)
	quit(save = 'no', status = code, runLast = FALSE)
}
" +
argumentSetup +
@"
read_original_metadata <- function(inFile) {
	lines <- readLines(inFile, warn = FALSE)

	header <- NULL
	type_row <- NULL
	annotation_rows <- list()

	for (line in lines) {
	if (is.null(header) && !startsWith(line, '#!')) {
		header <- strsplit(line, '\t', fixed = TRUE)[[1]]
		next
	}

	if (startsWith(line, '#!{Type}')) {
		type_text <- sub('^#!\\{Type\\}', '', line)
		type_row <- strsplit(type_text, '\t', fixed = TRUE)[[1]]
		next
	}

	if (startsWith(line, '#!{') && !startsWith(line, '#!{Type}')) {
		key <- sub('^#!\\{([^}]*)\\}.*$', '\\1', line)
		value_text <- sub('^#!\\{[^}]*\\}', '', line)
		annotation_rows[[key]] <- strsplit(value_text, '\t', fixed = TRUE)[[1]]
		next
	}
	}

	if (is.null(header) || is.null(type_row) || length(header) != length(type_row)) {
	return(NULL)
	}

	type_map <- type_row
	names(type_map) <- header

	list(
	header = header,
	type_map = type_map,
	annotation_rows = annotation_rows
	)
}

infer_type <- function(x) {
	if (is.numeric(x) || is.integer(x)) {
	return('N')
	}

	return('T')
}

write_full_perseus_table <- function(result_df, outFile, metadata) {
	result_df <- as.data.frame(result_df, stringsAsFactors = FALSE, check.names = FALSE)

	result_colnames <- colnames(result_df)
	types <- character(length(result_colnames))

	for (i in seq_along(result_colnames)) {
	col_name <- result_colnames[i]

	if (!is.null(metadata) && col_name %in% names(metadata$type_map)) {
		types[i] <- metadata$type_map[[col_name]]
	} else {
		types[i] <- infer_type(result_df[[i]])
	}
	}

	con <- file(outFile, open = 'wt', encoding = 'UTF-8')
	on.exit(close(con), add = TRUE)

	writeLines(paste(result_colnames, collapse = '\t'), con)
	writeLines(paste0('#!{Type}', paste(types, collapse = '\t')), con)

	if (!is.null(metadata) && length(metadata$annotation_rows) > 0) {
	for (key in names(metadata$annotation_rows)) {
		original_values <- metadata$annotation_rows[[key]]
		values <- character(length(result_colnames))

		for (i in seq_along(result_colnames)) {
		match_index <- match(result_colnames[i], metadata$header)
		if (!is.na(match_index) && match_index <= length(original_values)) {
			values[i] <- original_values[match_index]
		} else {
			values[i] <- ''
		}
		}

		writeLines(paste0('#!{', key, '}', paste(values, collapse = '\t')), con)
	}
	}

	utils::write.table(
	result_df,
	file = con,
	sep = '\t',
	row.names = FALSE,
	col.names = FALSE,
	quote = FALSE,
	na = 'NaN'
	)
}

metadata <- read_original_metadata(inFile)

df <- tryCatch({
	utils::read.delim(
	inFile,
	sep = '\t',
	header = TRUE,
	comment.char = '#',
	check.names = FALSE,
	stringsAsFactors = FALSE
	)
}, error = function(e) {
	fail(paste0('Failed to read full Perseus input table.', '\n', conditionMessage(e)))
})

tryCatch({
" + escapedUserCode + @"
}, error = function(e) {
	message('INLINE_USER_CODE_ERROR')
	fail(paste0('Error while executing user code.', '\n', conditionMessage(e)))
})

if (!exists('result_df')) {
	fail('Your code must assign a data.frame named result_df.')
}

if (!is.data.frame(result_df)) {
	fail('Your code must assign result_df as a data.frame.')
}

tryCatch({
	write_full_perseus_table(result_df, outFile, metadata)
}, error = function(e) {
	fail(paste0('Failed to write Perseus matrix output.', '\n', conditionMessage(e)))
})
";

			string tempScriptPath = Path.Combine(
				Path.GetTempPath(),
				"perseus_r_runtime_" + Guid.NewGuid().ToString("N") + ".R");

			File.WriteAllText(tempScriptPath, script, new UTF8Encoding(false));
			return tempScriptPath;
		}
	}
}