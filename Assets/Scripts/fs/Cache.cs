using System.Collections.Generic;

namespace fs {
    class Cache {

        private static Dictionary<string,PmdFile> pmd = new Dictionary<string,PmdFile>();
        private static Dictionary<string,TobjFile> tobj = new Dictionary<string,TobjFile>();
        private static Dictionary<string,MatFile> mat = new Dictionary<string,MatFile>();
        public static TobjFile LoadTobj(string path) {
            tobj.TryGetValue(path,out TobjFile tobjFile);
            if(tobjFile==null) {
                tobjFile = new TobjFile(path);
                tobj.Add(path,tobjFile);
            }
            return tobjFile;
        }

        public static MatFile LoadMat(string path) {
            mat.TryGetValue(path,out MatFile matFile);
            if(matFile==null) {
                matFile = new MatFile(path);
                mat.Add(path,matFile);
            }
            return matFile;
        }
    }
}
