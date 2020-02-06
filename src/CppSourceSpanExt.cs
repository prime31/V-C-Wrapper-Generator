using System.IO;
using CppAst;

namespace Generator
{
    public static class CppSourceSpanExt
    {
        public static string FilePath(this CppSourceSpan cppSpan)
        {
            var span = cppSpan.ToString();
            return span.Substring(0, span.IndexOf("("));
        }

        public static string Filename(this CppSourceSpan cppSpan)
        {
			return Path.GetFileName(cppSpan.FilePath());
        }

        public static string FilenameNoExtension(this CppSourceSpan cppSpan)
        {
			return cppSpan.Filename().Replace(".h", "");
        }

        public static string Folder(this CppSourceSpan cppSpan)
        {
			return Path.GetFileName(Path.GetDirectoryName(cppSpan.FilePath()));
        }

        public static string FolderFromBaseSrcFolder(this CppSourceSpan cppSpan, string baseSrcFolder)
        {
            var path = cppSpan.FilePath();
            if (cppSpan.FilePath().IndexOf(baseSrcFolder) == -1)
                return Path.GetFileName(Path.GetDirectoryName(path));
            return Path.GetDirectoryName(path.Substring(path.IndexOf(baseSrcFolder) + baseSrcFolder.Length + 1));
        }

        public static void CopyTo(this CppSourceSpan cppSpan, Config config)
        {
            var dst = Path.Combine(config.DstDir, "thirdparty", config.BaseSourceFolder, cppSpan.FolderFromBaseSrcFolder(config.BaseSourceFolder));

            // we could have a header outside of our BaseSourceFolder. in that case stick it in a folder in thirdparty
            if (cppSpan.FilePath().IndexOf(config.BaseSourceFolder) == -1)
                dst = Path.Combine(config.DstDir, "thirdparty", cppSpan.FolderFromBaseSrcFolder(config.BaseSourceFolder));
            Directory.CreateDirectory(dst);

            File.Copy(cppSpan.FilePath(), Path.Combine(dst, cppSpan.Filename()), true);
        }
    }
}