namespace MqUtil.Parse.Psp{
    public class PspPtmWriter{
        public string Accession { get; set; }
        public string Name { get; set; }
        public bool InVivo { get; set; }
        public bool InVitro { get; set; }

        public PspPtmWriter(string accession, string name, bool inVivo, bool inVitro){
            Accession = accession;
            Name = name;
            InVitro = inVitro;
            InVivo = inVivo;
        }

        public override string ToString(){
            return Accession + "," + Name + "," + InVivo + "," + InVitro;
        }
    }
}