using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DNA.IO
{
	public static class PathTools
	{
		public static string ReplaceInvalidChars(string orginalPath)
		{
			return PathTools.ReplaceInvalidChars(orginalPath, '_');
		}

		public static string ReplaceInvalidChars(string orginalPath, char replaceChar)
		{
			StringBuilder sb = new StringBuilder(orginalPath);
			for (int i = 0; i < sb.Length; i++)
			{
				char c = sb[i];
				if (c == '?' || c == '*')
				{
					sb[i] = replaceChar;
				}
				else
				{
					foreach (char ivc in Path.GetInvalidPathChars())
					{
						if (c == ivc)
						{
							sb[i] = replaceChar;
						}
					}
				}
			}
			return sb.ToString();
		}

		public static string GetTempFolderPath()
		{
			string pathName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(pathName);
			return pathName;
		}

		public static string GetTempFilePath()
		{
			return Path.Combine(Path.GetTempPath(), PathTools.GetTempFileName());
		}

		public static string GetTempFileName()
		{
			return Guid.NewGuid().ToString() + ".tmp";
		}

		public static string[] GetFileNames(string[] paths)
		{
			string[] result = new string[paths.Length];
			for (int i = 0; i < paths.Length; i++)
			{
				result[i] = Path.GetFileName(paths[i]);
			}
			return result;
		}

		public static string RootDirectory(string path)
		{
			int index = path.IndexOf(Path.DirectorySeparatorChar);
			if (index < 0)
			{
				return "";
			}
			if (index == 0)
			{
				path = path.Substring(1);
				return PathTools.RootDirectory(path);
			}
			string result = path.Substring(0, index);
			if (result == ".")
			{
				return PathTools.RootDirectory(path.Substring(index));
			}
			if (result.Length > 1 && result[1] == ':')
			{
				return PathTools.RootDirectory(path.Substring(index));
			}
			return result;
		}

		private static void InnerGetFiles(string dirname, string pattern, List<string> files)
		{
			foreach (string subdir in Directory.EnumerateDirectories(dirname))
			{
				PathTools.InnerGetFiles(subdir, pattern, files);
			}
			foreach (string file in Directory.EnumerateFiles(dirname, pattern))
			{
				files.Add(file);
			}
		}

		public static string[] RecursivelyGetFiles(string dirname, string pattern)
		{
			List<string> result = new List<string>();
			PathTools.InnerGetFiles(dirname, pattern, result);
			return result.ToArray();
		}

		public static Regex FilePatternToRegex(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException();
			}
			pattern = pattern.Trim();
			if (pattern.Length == 0)
			{
				throw new ArgumentException("Pattern is empty.");
			}
			if (PathTools.IlegalCharactersRegex.IsMatch(pattern))
			{
				throw new ArgumentException("Patterns contains ilegal characters.");
			}
			bool hasExtension = PathTools.CatchExtentionRegex.IsMatch(pattern);
			bool matchExact = false;
			if (PathTools.HasQuestionMarkRegEx.IsMatch(pattern))
			{
				matchExact = true;
			}
			else if (hasExtension)
			{
				matchExact = PathTools.CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
			}
			string regexString = Regex.Escape(pattern);
			regexString = "^" + Regex.Replace(regexString, "\\\\\\*", ".*");
			regexString = Regex.Replace(regexString, "\\\\\\?", ".");
			if (!matchExact && hasExtension)
			{
				regexString += PathTools.NonDotCharacters;
			}
			regexString += "$";
			return new Regex(regexString, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		public static bool IsBelow(string root, string path)
		{
			DirectoryInfo rootInfo = new DirectoryInfo(root);
			for (DirectoryInfo childInfo = new DirectoryInfo(path); childInfo != null; childInfo = childInfo.Parent)
			{
				if (childInfo.FullName == rootInfo.FullName)
				{
					return true;
				}
			}
			return false;
		}

		public static string MakeRelative(string root, string path)
		{
			root = root.TrimEnd(new char[] { Path.DirectorySeparatorChar });
			path = path.TrimEnd(new char[] { Path.DirectorySeparatorChar });
			DirectoryInfo rootInfo = new DirectoryInfo(root);
			DirectoryInfo childInfo = new DirectoryInfo(path);
			Stack<DirectoryInfo> rootDirs = new Stack<DirectoryInfo>();
			for (DirectoryInfo dinfo = rootInfo; dinfo != null; dinfo = dinfo.Parent)
			{
				rootDirs.Push(dinfo);
			}
			Stack<DirectoryInfo> childDirs = new Stack<DirectoryInfo>();
			for (DirectoryInfo dinfo = childInfo; dinfo != null; dinfo = dinfo.Parent)
			{
				childDirs.Push(dinfo);
			}
			if (childDirs.Count < rootDirs.Count)
			{
				throw new IOException("path must be under root");
			}
			while (rootDirs.Count > 0 && rootDirs.Peek().FullName == childDirs.Peek().FullName)
			{
				rootDirs.Pop();
				childDirs.Pop();
			}
			if (rootDirs.Count > 0)
			{
				throw new IOException("path must be under root");
			}
			string outputDir = "";
			while (childDirs.Count > 0)
			{
				outputDir = Path.Combine(outputDir, childDirs.Pop().Name);
			}
			return outputDir;
		}

		private static Regex HasQuestionMarkRegEx = new Regex("\\?", RegexOptions.Compiled);

		private static Regex IlegalCharactersRegex = new Regex("[\\/:<>|\"]", RegexOptions.Compiled);

		private static Regex CatchExtentionRegex = new Regex("^\\s*.+\\.([^\\.]+)\\s*$", RegexOptions.Compiled);

		private static string NonDotCharacters = "[^.]*";
	}
}
