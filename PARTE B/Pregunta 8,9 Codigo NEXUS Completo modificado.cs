using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NexusTraining
{
    public enum TipoObjeto
    {
        CelulaEnergia,
        Escudo,
        ModuloAtaque,
        Analizador,
        NucleoResonancia,
        ModuloDesconocido
    }

    public enum ResultadoEncuentro
    {
        Victoria,
        Escape
    }

    public static class Juego
    {
        public static Random Azar = new Random();

        public static void GameOver(Nexus nexus, string causa)
        {
            Console.Clear();
            Ui.Marco(ConsoleColor.DarkRed, "", "CONEXIÓN NEXUS PERDIDA", "", "G A M E   O V E R", "");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(causa);
            Console.WriteLine("Tu señal se apaga en la realidad desconocida...");
            Console.WriteLine();
            Console.WriteLine("Cadete: " + nexus.Cadete.Nombre);
            Console.WriteLine("Nivel alcanzado: " + nexus.Nivel + "   XP: " + nexus.Cadete.Xp);
            Console.WriteLine("Entidades derrotadas: " + nexus.EntidadesDerrotadas);
            Console.WriteLine("\nPresione ENTER para cerrar NEXUS...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }

    public static class Ui
    {
        public const int Ancho = 60;

        public static string Centrar(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            int izquierda = (ancho - texto.Length) / 2;
            int derecha = ancho - texto.Length - izquierda;
            return new string(' ', izquierda) + texto + new string(' ', derecha);
        }

        public static string Ajustar(string texto, int ancho)
        {
            if (texto.Length > ancho) return texto.Substring(0, ancho);
            return texto.PadRight(ancho);
        }

        public static void Escribir(string texto, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(texto);
        }

        public static void Pausa(string texto = "Presione ENTER para continuar...")
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n" + texto);
            Console.ReadLine();
        }

        public static void Titulo(string texto, ConsoleColor color)
        {
            Console.Clear();
            Console.ForegroundColor = color;
            Console.WriteLine(new string('═', 62));
            Console.WriteLine("  " + texto);
            Console.WriteLine(new string('═', 62));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        public static void Marco(ConsoleColor color, params string[] lineas)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("╔" + new string('═', Ancho) + "╗");
            foreach (string linea in lineas) Console.WriteLine("║" + Centrar(linea, Ancho) + "║");
            Console.WriteLine("╚" + new string('═', Ancho) + "╝");
        }

        public static string Barra(double valor, double maximo, int largo)
        {
            if (maximo <= 0) maximo = 1;
            int llenos = (int)Math.Round(valor / maximo * largo);
            if (llenos < 0) llenos = 0;
            if (llenos > largo) llenos = largo;
            return new string('█', llenos) + new string('░', largo - llenos);
        }

        public static ConsoleColor ColorVida(double valor, double maximo)
        {
            double porcentaje = valor / maximo;
            if (porcentaje > 0.6) return ConsoleColor.Green;
            if (porcentaje > 0.3) return ConsoleColor.Yellow;
            return ConsoleColor.Red;
        }

        public static bool Confirmar(string pregunta)
        {
            Console.Write(pregunta);
            string respuesta = Console.ReadLine();
            if (respuesta == null) return false;
            return respuesta.Trim().ToUpper() == "S";
        }
    }

    // PREGUNTA 8
    public interface IEstadoConsultable
    {
        string ObtenerResumenEstado();
    }

    public abstract class Personaje
    {
        public string Nombre { get; protected set; }
        public double Energia { get; protected set; }
        public double EnergiaMaxima { get; protected set; }
        public bool PuntoDebilExpuesto { get; set; }
        public abstract string Tipo { get; }
        public virtual int ProbabilidadEscape { get { return 50; } }
        public bool EstaVivo { get { return Energia > 0; } }

        protected Personaje(string nombre, double energia, double energiaMaxima)
        {
            Nombre = nombre;
            Energia = energia;
            EnergiaMaxima = energiaMaxima;
        }

        public virtual double RecibirDano(double cantidad)
        {
            if (cantidad < 0) cantidad = 0;
            double antes = Energia;
            Energia = Energia - cantidad;
            if (Energia < 0) Energia = 0;
            return antes - Energia;
        }

        public abstract void MostrarFicha();

        public virtual void AlEncontrarse(Cadete cadete)
        {
        }

        public virtual void Turno(Cadete cadete)
        {
        }
    }

    // PREGUNTA 8
    public class Cadete : Personaje, IEstadoConsultable
    {
        public int Edad { get; set; }
        public string Realidad { get; set; }
        public double Estabilidad { get; private set; }
        public int Xp { get; private set; }
        public double Escudo { get; private set; }
        public int CargasModulo { get; private set; }
        public double UltimoAbsorbido { get; private set; }
        public bool UltimoAtaqueConModulo { get; private set; }
        public bool UltimoAtaqueAPuntoDebil { get; private set; }
        public int Rango { get { return Xp / 100; } }
        public override string Tipo { get { return "CADETE"; } }

        public Cadete(string nombre, int edad, string realidad, double energia, double estabilidad)
            : base(nombre, energia, 100)
        {
            Edad = edad;
            Realidad = realidad;
            Estabilidad = estabilidad;
        }

        public void Explorar()
        {
            Console.WriteLine("Exploración iniciada.");
        }

        public void AnalizarAnomalia()
        {
            Energia = Energia - 10;
            if (Estabilidad >= 60) Estabilidad = Estabilidad + 5;
            else Estabilidad = Estabilidad - 5;
            if (Estabilidad > 100) Estabilidad = 100;
            if (Estabilidad < 0) Estabilidad = 0;
        }

        public void RecuperarEnergia()
        {
            Energia = Energia + 25;
            if (Energia > 100) Energia = 100;
            Estabilidad = Estabilidad - 5;
            if (Estabilidad < 0) Estabilidad = 0;
        }

        public void EstabilizarEnlace()
        {
            Energia = Energia - 15;
            Estabilidad = Estabilidad + 15;
            if (Energia < 0) Energia = 0;
            if (Estabilidad > 100) Estabilidad = 100;
        }

        public void AumentarEstabilidad(double cantidad)
        {
            Estabilidad = Estabilidad + cantidad;
            if (Estabilidad > 100) Estabilidad = 100;
            if (Estabilidad < 0) Estabilidad = 0;
        }

        // Pregunta 10
        public void AgregarEnergia(double cantidad)
        {
            if (cantidad < 0) return;

            Energia = Energia + cantidad;
            if (Energia > EnergiaMaxima) Energia = EnergiaMaxima;
        }

        public void PerderEnergia(double cantidad)
        {
            Energia = Energia - cantidad;
            if (Energia < 0) Energia = 0;
        }

        public double PerderEnergiaNoLetal(double cantidad)
        {
            double perdida = cantidad;
            if (perdida > Energia - 1) perdida = Energia - 1;
            if (perdida < 0) perdida = 0;
            Energia = Energia - perdida;
            return perdida;
        }

        public void PerderEstabilidad(double cantidad)
        {
            Estabilidad = Estabilidad - cantidad;
            if (Estabilidad < 0) Estabilidad = 0;
        }

        public void ActivarEscudo(double puntos)
        {
            Escudo = Escudo + puntos;
            if (Escudo > 50) Escudo = 50;
        }

        public void CargarModulo(int cargas)
        {
            CargasModulo = CargasModulo + cargas;
            if (CargasModulo > 6) CargasModulo = 6;
        }

        public void GanarXp(int cantidad)
        {
            Xp = Xp + cantidad;
        }

        public override double RecibirDano(double cantidad)
        {
            if (cantidad < 0) cantidad = 0;
            UltimoAbsorbido = 0;
            if (Escudo > 0)
            {
                double absorbido = Math.Min(Escudo, cantidad);
                Escudo = Escudo - absorbido;
                cantidad = cantidad - absorbido;
                UltimoAbsorbido = absorbido;
            }
            return base.RecibirDano(cantidad);
        }

        public double Atacar(Personaje objetivo)
        {
            double dano = 10 + Rango * 2 + Juego.Azar.Next(0, 6);
            UltimoAtaqueConModulo = false;
            UltimoAtaqueAPuntoDebil = false;
            if (CargasModulo > 0)
            {
                dano = dano + 10;
                CargasModulo--;
                UltimoAtaqueConModulo = true;
            }
            if (objetivo.PuntoDebilExpuesto)
            {
                dano = Math.Round(dano * 1.5);
                UltimoAtaqueAPuntoDebil = true;
            }
            double real = objetivo.RecibirDano(dano);
            objetivo.PuntoDebilExpuesto = false;
            return real;
        }

        public bool IntentarEscapar(Personaje entidad)
        {
            int probabilidad = entidad.ProbabilidadEscape;
            if (Estabilidad >= 60) probabilidad = probabilidad + 10;
            if (Estabilidad < 30) probabilidad = probabilidad - 10;
            return Juego.Azar.Next(100) < probabilidad;
        }

        public override void MostrarFicha()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n─────── ESTADISTICAS DEL EXPLORADOR ───────");
            Console.WriteLine("Nombre:      " + Nombre);
            Console.WriteLine("Edad:        " + Edad);
            Console.WriteLine("Energía:     " + Energia + "/" + EnergiaMaxima);
            Console.WriteLine("Estabilidad: " + Estabilidad + "/100");
            Console.WriteLine("Escudo:      " + Escudo);
            Console.WriteLine("XP:          " + Xp + "  (Rango " + Rango + ")");
        }

        // PREGUNTA 8
        public string ObtenerResumenEstado()
        {
            return "Cadete: " + Nombre + " | Energía actual: " + Energia;
        }
    }

    // PREGUNTA 8
    public class Robot : Personaje, IEstadoConsultable
    {
        public int Descarga { get; private set; }
        public bool DefensaActiva { get; private set; }
        public override string Tipo { get { return "ROBOT"; } }
        public override int ProbabilidadEscape { get { return 70; } }

        public Robot(string nombre, double energia, int descarga)
            : base(nombre, energia, energia)
        {
            Descarga = descarga;
        }

        public void ActivarSistemaDefensivo()
        {
            DefensaActiva = true;
        }

        public override void AlEncontrarse(Cadete cadete)
        {
            ActivarSistemaDefensivo();
            Ui.Escribir("\n🤖 " + Nombre + " detecta tu presencia...", ConsoleColor.Yellow);
            Console.WriteLine("🛡️ SISTEMA DEFENSIVO ACTIVADO: su blindaje reducirá tus ataques.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public override double RecibirDano(double cantidad)
        {
            if (DefensaActiva)
            {
                if (PuntoDebilExpuesto) Ui.Escribir("💥 ¡El analizador anula el sistema defensivo!", ConsoleColor.Yellow);
                else
                {
                    cantidad = Math.Round(cantidad * 0.4);
                    Ui.Escribir(" El sistema defensivo amortigua el impacto (-60% de daño).", ConsoleColor.Yellow);
                }
            }
            DefensaActiva = false;
            return base.RecibirDano(cantidad);
        }

        public double Atacar(Cadete cadete)
        {
            double dano = Descarga + Juego.Azar.Next(0, 4);
            return cadete.RecibirDano(dano);
        }

        public override void Turno(Cadete cadete)
        {
            Console.WriteLine();
            double real = Atacar(cadete);
            Ui.Escribir("⚡ " + Nombre + " lanza una descarga: -" + real + " de energía.", ConsoleColor.Yellow);
            if (cadete.UltimoAbsorbido > 0) Ui.Escribir("🛡️ Tu escudo absorbió " + cadete.UltimoAbsorbido + " de daño.", ConsoleColor.Cyan);
            if (DefensaActiva == false && Juego.Azar.Next(100) < 55)
            {
                ActivarSistemaDefensivo();
                Ui.Escribir("🛡️ " + Nombre + " reactiva su sistema defensivo.", ConsoleColor.Yellow);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public override void MostrarFicha()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n─────── ANÁLISIS DE ENTIDAD ───────");
            Console.WriteLine("Tipo:        " + Tipo);
            Console.WriteLine("Nombre:      " + Nombre);
            Console.WriteLine("Energía:     " + Energia + "/" + EnergiaMaxima);
            Console.WriteLine("Descarga:    " + Descarga + " a " + (Descarga + 3) + " de daño");
            if (DefensaActiva) Console.WriteLine("Defensa:     ACTIVA (-60% al daño recibido)");
            else Console.WriteLine("Defensa:     inactiva");
            Console.WriteLine("Conducta:    activa un sistema defensivo y responde con descargas.");
            if (PuntoDebilExpuesto) Console.WriteLine("Punto débil: EXPUESTO (+50% de daño e ignora su defensa)");
            else Console.WriteLine("Punto débil: sin identificar (usa un ANALIZADOR)");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // PREGUNTA 8
        public string ObtenerResumenEstado()
        {
            return "Robot: " + Nombre + " | Energía actual: " + Energia;
        }
    }

    public class Enemigo : Personaje
    {
        private int turnos;
        public int Ataque { get; private set; }
        public string Habilidad { get; private set; }
        public bool EsJefe { get; private set; }

        public override string Tipo
        {
            get
            {
                if (EsJefe) return "ENEMIGO (JEFE)";
                return "ENEMIGO";
            }
        }

        public override int ProbabilidadEscape
        {
            get
            {
                if (EsJefe) return 30;
                return 45;
            }
        }

        public bool Enfurecido { get { return Energia > 0 && Energia <= EnergiaMaxima * 0.3; } }

        public Enemigo(string nombre, double vida, int ataque, string habilidad, bool esJefe)
            : base(nombre, vida, vida)
        {
            Ataque = ataque;
            Habilidad = habilidad;
            EsJefe = esJefe;
        }

        public override void AlEncontrarse(Cadete cadete)
        {
            Ui.Escribir("\n⚔ ¡" + Nombre + " se abalanza sobre ti! COMBATE INICIADO.", ConsoleColor.Red);
            if (EsJefe == false && Juego.Azar.Next(100) < 35)
            {
                Console.WriteLine("¡EMBOSCADA! La entidad ataca primero.");
                Turno(cadete);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public double Atacar(Cadete cadete)
        {
            turnos++;
            Console.ForegroundColor = ConsoleColor.Red;
            double dano = Ataque + Juego.Azar.Next(0, 6);
            if (Enfurecido)
            {
                dano = Math.Round(dano * 1.3);
                Console.WriteLine("💢 " + Nombre + " entra en FURIA (+30% de daño).");
            }
            if (EsJefe && turnos % 3 == 0)
            {
                dano = Math.Round(dano * 1.6);
                Console.WriteLine("⚡ ¡PULSO DE ANOMALÍA! El guardián libera su energía acumulada.");
            }
            double real = cadete.RecibirDano(dano);
            if (Habilidad == "DRENAJE" && real > 0)
            {
                double cura = Math.Round(real / 2);
                Energia = Math.Min(EnergiaMaxima, Energia + cura);
                Console.WriteLine("🩸 " + Nombre + " drena energía y recupera " + cura + ".");
            }
            else if (Habilidad == "INTERFERENCIA")
            {
                cadete.PerderEstabilidad(3);
                Console.WriteLine("📡 Interferencia: -3 de estabilidad.");
            }
            return real;
        }

        public override void Turno(Cadete cadete)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            double real = Atacar(cadete);
            Ui.Escribir("👾 " + Nombre + " te ataca: -" + real + " de energía.", ConsoleColor.Red);
            if (cadete.UltimoAbsorbido > 0) Ui.Escribir("🛡️ Tu escudo absorbió " + cadete.UltimoAbsorbido + " de daño.", ConsoleColor.Cyan);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public override void MostrarFicha()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n─────── ANÁLISIS DE ENTIDAD ───────");
            Console.WriteLine("Tipo:        " + Tipo);
            Console.WriteLine("Nombre:      " + Nombre);
            Console.WriteLine("Energía:     " + Energia + "/" + EnergiaMaxima);
            Console.WriteLine("Ataque:      " + Ataque + " a " + (Ataque + 5) + " de daño");
            string habilidad = "ninguna";
            if (Habilidad == "DRENAJE") habilidad = "Drenaje (recupera energía al herirte)";
            else if (Habilidad == "INTERFERENCIA") habilidad = "Interferencia (reduce tu estabilidad)";
            else if (Habilidad == "PULSO") habilidad = "Pulso de anomalía (golpe devastador cada 3 turnos)";
            Console.WriteLine("Habilidad:   " + habilidad);
            Console.WriteLine("Conducta:    agresivo; se enfurece con poca energía.");
            if (PuntoDebilExpuesto) Console.WriteLine("Punto débil: EXPUESTO (+50% de daño en el próximo ataque)");
            else Console.WriteLine("Punto débil: sin identificar (usa un ANALIZADOR)");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static class Entidades
    {
        public static Enemigo CrearEnemigo(int nivel)
        {
            int bono = nivel - 1;
            int tipo = Juego.Azar.Next(3);
            if (tipo == 0) return new Enemigo("SOMBRA DEL VACÍO", 35 + bono * 5, 7 + bono, "INTERFERENCIA", false);
            if (tipo == 1) return new Enemigo("DEVORADOR DE ENERGÍA", 40 + bono * 5, 6 + bono, "DRENAJE", false);
            return new Enemigo("ESPECTRO IRIS", 30 + bono * 5, 9 + bono, "NINGUNA", false);
        }

        public static Robot CrearRobot(int nivel)
        {
            int bono = nivel - 1;
            int tipo = Juego.Azar.Next(3);
            if (tipo == 0) return new Robot("DRON VIGÍA", 40 + bono * 5, 5 + bono);
            if (tipo == 1) return new Robot("AUTÓMATA GUARDIÁN", 55 + bono * 5, 6 + bono);
            return new Robot("CENTINELA MK-II", 45 + bono * 5, 7 + bono);
        }

        public static Enemigo CrearGuardian(int nivel)
        {
            int bono = nivel - 1;
            return new Enemigo("GUARDIÁN DE LA ANOMALÍA", 60 + bono * 5, 9 + bono, "PULSO", true);
        }

        public static Personaje CrearAleatoria(int nivel)
        {
            if (Juego.Azar.Next(100) < 55) return CrearEnemigo(nivel);
            return CrearRobot(nivel);
        }
    }

    public class Objeto
    {
        public string Nombre { get; private set; }
        public TipoObjeto Tipo { get; private set; }
        public string Icono { get; private set; }
        public string Descripcion { get; private set; }

        public Objeto(string nombre, TipoObjeto tipo, string icono, string descripcion)
        {
            Nombre = nombre;
            Tipo = tipo;
            Icono = icono;
            Descripcion = descripcion;
        }

        public static Objeto Crear(TipoObjeto tipo)
        {
            if (tipo == TipoObjeto.CelulaEnergia) return new Objeto("CÉLULA DE ENERGÍA", tipo, "🔋", "Restaura 25 de energía");
            if (tipo == TipoObjeto.Escudo) return new Objeto("ESCUDO", tipo, "🛡️", "Absorbe 25 de daño");
            if (tipo == TipoObjeto.ModuloAtaque) return new Objeto("MÓDULO DE ATAQUE", tipo, "🔫", "+10 daño x3 ataques");
            if (tipo == TipoObjeto.Analizador) return new Objeto("ANALIZADOR", tipo, "🔬", "Expone el punto débil");
            if (tipo == TipoObjeto.NucleoResonancia) return new Objeto("NÚCLEO DE RESONANCIA", tipo, "◆", "Objetivo de misión");
            return new Objeto("MÓDULO DESCONOCIDO", tipo, "❖", "+30 energía +15 estab.");
        }

        public static Objeto Aleatorio()
        {
            int tirada = Juego.Azar.Next(100);
            if (tirada < 35) return Crear(TipoObjeto.CelulaEnergia);
            if (tirada < 60) return Crear(TipoObjeto.Escudo);
            if (tirada < 80) return Crear(TipoObjeto.ModuloAtaque);
            return Crear(TipoObjeto.Analizador);
        }
    }

    public class Inventario
    {
        public const int Capacidad = 8;
        private List<Objeto> objetos = new List<Objeto>();
        public string Error { get; private set; }
        public int Cantidad { get { return objetos.Count; } }
        public bool EstaLleno { get { return objetos.Count >= Capacidad; } }
        public List<Objeto> Lista { get { return new List<Objeto>(objetos); } }

        public bool Agregar(Objeto objeto)
        {
            if (objeto == null || EstaLleno) return false;
            objetos.Add(objeto);
            return true;
        }

        public bool Contiene(Objeto objeto)
        {
            return objetos.Contains(objeto);
        }

        public bool Tiene(TipoObjeto tipo)
        {
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Tipo == tipo) return true;
            }
            return false;
        }

        public Objeto Buscar(string entrada)
        {
            Error = "";
            if (string.IsNullOrWhiteSpace(entrada))
            {
                Error = "No ingresaste ningún objeto.";
                return null;
            }
            entrada = entrada.Trim();
            int numero;
            if (int.TryParse(entrada, out numero))
            {
                if (numero < 1 || numero > objetos.Count)
                {
                    Error = "El objeto #" + numero + " no existe en tu inventario.";
                    return null;
                }
                return objetos[numero - 1];
            }
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Nombre.ToUpper().Contains(entrada.ToUpper())) return objeto;
            }
            Error = "No posees ningún objeto llamado '" + entrada + "'.";
            return null;
        }

        public bool Eliminar(Objeto objeto)
        {
            return objetos.Remove(objeto);
        }

        public void EliminarTipo(TipoObjeto tipo)
        {
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Tipo == tipo)
                {   
                    objetos.Remove(objeto);
                    return;
                }
            }
        }

        // Pregunta 9
        public static List<string> ObtenerNombresPorTipo(List<Objeto> objetos, TipoObjeto tipo)
        {
            return objetos
                .Where(objeto => objeto.Tipo == tipo)
                .Select(objeto => objeto.Nombre)
                .OrderBy(nombre => nombre)
                .ToList();
        }

        public void MostrarLista()
        {
            if (objetos.Count == 0)
            {
                Console.WriteLine("║" + new string(' ', Ui.Ancho) + "║");
                Console.WriteLine("║" + Ui.Ajustar("              INVENTARIO VACÍO", Ui.Ancho) + "║");
                Console.WriteLine("║" + new string(' ', Ui.Ancho) + "║");
            }
            else
            {
                for (int i = 0; i < objetos.Count; i++)
                {
                    string fila = "  [" + (i + 1) + "] " + objetos[i].Nombre.PadRight(24) + " " + objetos[i].Descripcion;
                    Console.WriteLine("║" + Ui.Ajustar(fila, Ui.Ancho) + "║");
                }
            }
            Console.WriteLine("║" + new string(' ', Ui.Ancho) + "║");
            Console.WriteLine("║" + Ui.Ajustar("  Capacidad: " + objetos.Count + "/" + Capacidad, Ui.Ancho) + "║");
            Console.WriteLine("╚" + new string('═', Ui.Ancho) + "╝");
        }

        public void MostrarCompacta()
        {
            for (int i = 0; i < objetos.Count; i++) Console.WriteLine("  [" + (i + 1) + "] " + objetos[i].Icono + " " + objetos[i].Nombre + " - " + objetos[i].Descripcion);
        }
    }

    public class Objetivo
    {
        public string Descripcion { get; private set; }
        public string Clave { get; private set; }
        public bool Completado { get; set; }

        public Objetivo(string descripcion, string clave)
        {
            Descripcion = descripcion;
            Clave = clave;
        }
    }

    public class Mision
    {
        public string Titulo { get; private set; }
        public string Descripcion { get; private set; }
        public int RecompensaXp { get; private set; }
        public List<Objetivo> Objetivos { get; private set; }
        public bool RecompensaEntregada { get; set; }

        public bool Completada
        {
            get
            {
                foreach (Objetivo objetivo in Objetivos)
                {
                    if (objetivo.Completado == false) return false;
                }
                return true;
            }
        }

        public Objetivo Pendiente
        {
            get
            {
                foreach (Objetivo objetivo in Objetivos)
                {
                    if (objetivo.Completado == false) return objetivo;
                }
                return null;
            }
        }

        public Mision(string titulo, string descripcion, int recompensaXp)
        {
            Titulo = titulo;
            Descripcion = descripcion;
            RecompensaXp = recompensaXp;
            Objetivos = new List<Objetivo>();
        }

        public bool Notificar(string clave)
        {
            Objetivo pendiente = Pendiente;
            if (pendiente == null || pendiente.Clave != clave) return false;
            pendiente.Completado = true;
            return true;
        }
    }

    public class Nexus
    {
        public Cadete Cadete { get; private set; }
        public int Nivel { get; private set; }
        public int Anomalias { get; private set; }
        public bool ResonanciaActivada { get; private set; }
        public int EntidadesDerrotadas { get; set; }
        public Inventario Inventario { get; private set; }
        public Mision MisionPrincipal { get; private set; }

        public Nexus(Cadete cadete)
        {
            Cadete = cadete;
            Nivel = 1;
            Anomalias = 0;
            ResonanciaActivada = false;
            Inventario = new Inventario();
            MisionPrincipal = new Mision(
                "LA ANOMALÍA PERDIDA",
                "Investiga la anomalía detectada en una realidad desconocida y regresa con vida.",
                100);
            MisionPrincipal.Objetivos.Add(new Objetivo("Llegar a la zona de anomalía (⚠)", "anomalia"));
            MisionPrincipal.Objetivos.Add(new Objetivo("Encontrar el núcleo energético (◆)", "nucleo"));
            MisionPrincipal.Objetivos.Add(new Objetivo("Sobrevivir al encuentro con la entidad", "entidad"));
            MisionPrincipal.Objetivos.Add(new Objetivo("Regresar al punto de extracción (⌂)", "extraccion"));
            Inventario.Agregar(Objeto.Crear(TipoObjeto.CelulaEnergia));
            Inventario.Agregar(Objeto.Crear(TipoObjeto.Escudo));
            Inventario.Agregar(Objeto.Crear(TipoObjeto.Analizador));
        }

        public void ActivarResonancia()
        {
            ResonanciaActivada = true;
        }

        public void SubirNivel()
        {
            Nivel++;
        }

        public void AgregarAnomalia()
        {
            Anomalias++;
        }

        public void EliminarAnomalia()
        {
            if (Anomalias > 0) Anomalias--;
        }

        public void GanarXp(int cantidad)
        {
            int rangoAntes = Cadete.Rango;
            Cadete.GanarXp(cantidad);
            Ui.Escribir("+" + cantidad + " XP   (XP total: " + Cadete.Xp + ")", ConsoleColor.Green);
            if (Cadete.Rango > rangoAntes) Ui.Escribir("★ RANGO " + Cadete.Rango + " ALCANZADO: tu ataque base aumenta.", ConsoleColor.Magenta);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public bool RecogerObjeto(Objeto objeto)
        {
            if (Inventario.Agregar(objeto)) return true;
            Ui.Escribir("\n⚠ INVENTARIO LLENO (" + Inventario.Cantidad + "/" + Inventario.Capacidad + ")", ConsoleColor.Yellow);
            if (Ui.Confirmar("¿Descartar un objeto para llevarte " + objeto.Nombre + "? (S/N): "))
            {
                Console.WriteLine();
                Inventario.MostrarCompacta();
                Console.Write("\nNúmero o nombre del objeto a descartar: ");
                Objeto descartado = Inventario.Buscar(Console.ReadLine());
                if (descartado == null) Ui.Escribir("✖ " + Inventario.Error, ConsoleColor.Red);
                else if (descartado.Tipo == TipoObjeto.NucleoResonancia && MisionPrincipal.Completada == false) Ui.Escribir("✖ No puedes descartar el núcleo: lo necesitas para la misión.", ConsoleColor.Red);
                else
                {
                    Inventario.Eliminar(descartado);
                    Inventario.Agregar(objeto);
                    Ui.Escribir("✓ Descartaste " + descartado.Nombre + " y recogiste " + objeto.Nombre + ".", ConsoleColor.Green);
                    return true;
                }
            }
            Ui.Escribir("Dejas " + objeto.Nombre + " atrás.", ConsoleColor.DarkGray);
            return false;
        }

        public bool UsarObjeto(Objeto objeto, Personaje entidad)
        {
            if (objeto == null || Inventario.Contiene(objeto) == false)
            {
                Ui.Escribir("✖ No posees ese objeto.", ConsoleColor.Red);
                return false;
            }
            bool consumido = false;
            if (objeto.Tipo == TipoObjeto.CelulaEnergia) consumido = UsarCelula();
            else if (objeto.Tipo == TipoObjeto.Escudo) consumido = UsarEscudo();
            else if (objeto.Tipo == TipoObjeto.ModuloAtaque) consumido = UsarModuloAtaque();
            else if (objeto.Tipo == TipoObjeto.Analizador) consumido = UsarAnalizador(entidad);
            else if (objeto.Tipo == TipoObjeto.NucleoResonancia) consumido = UsarNucleo();
            else consumido = UsarModuloDesconocido();
            if (consumido) Inventario.Eliminar(objeto);
            Console.ForegroundColor = ConsoleColor.White;
            return consumido;
        }

        private bool UsarCelula()
        {
            if (Cadete.Energia >= Cadete.EnergiaMaxima)
            {
                Ui.Escribir("Tu energía ya está al máximo. La célula no se consume.", ConsoleColor.Yellow);
                return false;
            }
            double antes = Cadete.Energia;
            Cadete.AgregarEnergia(25);
            Ui.Escribir("🔋 Utilizas una CÉLULA DE ENERGÍA.", ConsoleColor.Green);
            Ui.Escribir("Energía: " + antes + " → " + Cadete.Energia, ConsoleColor.Green);
            return true;
        }

        private bool UsarEscudo()
        {
            if (Cadete.Escudo >= 50)
            {
                Ui.Escribir("Tu escudo ya está al máximo. No se consume.", ConsoleColor.Yellow);
                return false;
            }
            double antes = Cadete.Escudo;
            Cadete.ActivarEscudo(25);
            Ui.Escribir("🛡️ Activas un ESCUDO.", ConsoleColor.Cyan);
            Ui.Escribir("Escudo: " + antes + " → " + Cadete.Escudo, ConsoleColor.Cyan);
            return true;
        }

        private bool UsarModuloAtaque()
        {
            if (Cadete.CargasModulo >= 6)
            {
                Ui.Escribir("Tu módulo de ataque ya está al máximo de cargas.", ConsoleColor.Yellow);
                return false;
            }
            int antes = Cadete.CargasModulo;
            Cadete.CargarModulo(3);
            Ui.Escribir("🔫 Instalas un MÓDULO DE ATAQUE: +10 de daño en tus próximos ataques.", ConsoleColor.Yellow);
            Ui.Escribir("Cargas: " + antes + " → " + Cadete.CargasModulo, ConsoleColor.Yellow);
            return true;
        }

        private bool UsarAnalizador(Personaje entidad)
        {
            if (entidad == null)
            {
                Ui.Escribir("🔬 No hay ninguna entidad que analizar. El analizador no se consume.", ConsoleColor.Yellow);
                return false;
            }
            Ui.Escribir("🔬 Utilizas el ANALIZADOR sobre " + entidad.Nombre + "...", ConsoleColor.Cyan);
            Thread.Sleep(600);
            entidad.PuntoDebilExpuesto = true;
            entidad.MostrarFicha();
            Ui.Escribir("🎯 PUNTO DÉBIL EXPUESTO: tu próximo ataque hará +50% de daño.", ConsoleColor.Green);
            return true;
        }

        private bool UsarNucleo()
        {
            if (MisionPrincipal.Completada == false)
            {
                Ui.Escribir("🔒 El NÚCLEO DE RESONANCIA es el objetivo de tu misión.", ConsoleColor.Yellow);
                Ui.Escribir("Entrégalo en el punto de extracción (⌂) para completarla.", ConsoleColor.Yellow);
                return false;
            }
            double antes = Cadete.Energia;
            Cadete.AgregarEnergia(20);
            Cadete.AumentarEstabilidad(10);
            Ui.Escribir("◆ Absorbes la energía del NÚCLEO DE RESONANCIA.", ConsoleColor.Magenta);
            Ui.Escribir("Energía: " + antes + " → " + Cadete.Energia + "   (+10 estabilidad)", ConsoleColor.Magenta);
            return true;
        }

        private bool UsarModuloDesconocido()
        {
            double antes = Cadete.Energia;
            Cadete.AgregarEnergia(30);
            Cadete.AumentarEstabilidad(15);
            Ui.Escribir("❖ El MÓDULO DESCONOCIDO se integra a tu traje.", ConsoleColor.Magenta);
            Ui.Escribir("Energía: " + antes + " → " + Cadete.Energia + "   (+15 estabilidad)", ConsoleColor.Magenta);
            return true;
        }

        public bool UsarObjetoMenu(Personaje entidad, bool mostrarLista)
        {
            if (Inventario.Cantidad == 0)
            {
                Ui.Escribir("No tienes objetos para utilizar.", ConsoleColor.Yellow);
                return false;
            }
            if (mostrarLista)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Inventario.MostrarCompacta();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nNúmero o nombre del objeto a utilizar (ENTER para cancelar): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada))
            {
                Ui.Escribir("Acción cancelada.", ConsoleColor.DarkGray);
                return false;
            }
            Objeto objeto = Inventario.Buscar(entrada);
            if (objeto == null)
            {
                Ui.Escribir("✖ " + Inventario.Error, ConsoleColor.Red);
                return false;
            }
            Console.WriteLine();
            return UsarObjeto(objeto, entidad);
        }

        private void EliminarObjetoMenu()
        {
            if (Inventario.Cantidad == 0)
            {
                Ui.Escribir("No tienes objetos para eliminar.", ConsoleColor.Yellow);
                return;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nNúmero o nombre del objeto a eliminar (ENTER para cancelar): ");
            string entrada = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(entrada))
            {
                Ui.Escribir("Acción cancelada.", ConsoleColor.DarkGray);
                return;
            }
            Objeto objeto = Inventario.Buscar(entrada);
            if (objeto == null)
            {
                Ui.Escribir("✖ " + Inventario.Error, ConsoleColor.Red);
                return;
            }
            if (objeto.Tipo == TipoObjeto.NucleoResonancia && MisionPrincipal.Completada == false)
            {
                Ui.Escribir("✖ No puedes eliminar el núcleo: es necesario para completar la misión.", ConsoleColor.Red);
                return;
            }
            if (Ui.Confirmar("¿Seguro que deseas eliminar " + objeto.Nombre + "? (S/N): "))
            {
                Inventario.Eliminar(objeto);
                Ui.Escribir("✓ " + objeto.Nombre + " eliminado del inventario.", ConsoleColor.Green);
            }
            else Ui.Escribir("Eliminación cancelada.", ConsoleColor.DarkGray);
        }

        private void DibujarInventario()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Arte.Inventario);
            Console.WriteLine("╠" + new string('═', Ui.Ancho) + "╣");
            Inventario.MostrarLista();
            Console.ResetColor();
        }

        public void MostrarInventario()
        {
            bool salir = false;
            while (salir == false)
            {
                DibujarInventario();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n  [1] Utilizar objeto    [2] Eliminar objeto    [3] Volver");
                Console.Write("\nSeleccione una opción: ");
                int opcion;
                if (int.TryParse(Console.ReadLine(), out opcion) == false)
                {
                    Ui.Escribir("\n✖ Opción inválida. Ingresa un número del 1 al 3.", ConsoleColor.Red);
                    Thread.Sleep(1200);
                    continue;
                }
                if (opcion == 1)
                {
                    UsarObjetoMenu(null, false);
                    Ui.Pausa();
                }
                else if (opcion == 2)
                {
                    EliminarObjetoMenu();
                    Ui.Pausa();
                }
                else if (opcion == 3) salir = true;
                else
                {
                    Ui.Escribir("\n✖ Opción no válida.", ConsoleColor.Red);
                    Thread.Sleep(1200);
                }
            }
            Console.ResetColor();
        }

        public bool NotificarMision(string clave)
        {
            if (MisionPrincipal.Notificar(clave) == false) return false;
            Objetivo completado = null;
            foreach (Objetivo objetivo in MisionPrincipal.Objetivos)
            {
                if (objetivo.Clave == clave) completado = objetivo;
            }
            Ui.Escribir("\n✔ OBJETIVO COMPLETADO: " + completado.Descripcion, ConsoleColor.Green);
            Objetivo siguiente = MisionPrincipal.Pendiente;
            if (siguiente != null) Ui.Escribir("▶ Nuevo objetivo: " + siguiente.Descripcion, ConsoleColor.Cyan);
            Console.ForegroundColor = ConsoleColor.White;
            if (MisionPrincipal.Completada && MisionPrincipal.RecompensaEntregada == false) EntregarRecompensa();
            return true;
        }

        private void EntregarRecompensa()
        {
            MisionPrincipal.RecompensaEntregada = true;
            Inventario.EliminarTipo(TipoObjeto.NucleoResonancia);
            Thread.Sleep(900);
            Console.Clear();
            Ui.Marco(ConsoleColor.Green, "", "M I S I Ó N   C O M P L E T A D A", MisionPrincipal.Titulo, "");
            Console.WriteLine();
            Console.WriteLine("🟢 MISIÓN COMPLETADA");
            Console.WriteLine();
            GanarXp(MisionPrincipal.RecompensaXp);
            Ui.Escribir("+1 módulo desconocido", ConsoleColor.Magenta);
            RecogerObjeto(Objeto.Crear(TipoObjeto.ModuloDesconocido));
            Ui.Escribir("\nHas regresado con vida al punto de extracción.", ConsoleColor.White);
            Ui.Escribir("Puedes seguir explorando las realidades de NEXUS.", ConsoleColor.DarkGray);
            Ui.Pausa();
        }

        public void MostrarMisiones()
        {
            Console.Clear();
            Ui.Marco(ConsoleColor.Cyan, "NEXUS // MISIONES");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("  📋 " + MisionPrincipal.Titulo);
            Console.WriteLine("  " + MisionPrincipal.Descripcion);
            Console.WriteLine();
            Objetivo pendiente = MisionPrincipal.Pendiente;
            foreach (Objetivo objetivo in MisionPrincipal.Objetivos)
            {
                string marca = "[ ]";
                if (objetivo.Completado)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    marca = "[✔]";
                }
                else if (objetivo == pendiente)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    marca = "[▶]";
                }
                else Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("   " + marca + " " + objetivo.Descripcion);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("  Recompensa: +" + MisionPrincipal.RecompensaXp + " XP y +1 módulo desconocido");
            Console.WriteLine();
            if (MisionPrincipal.Completada) Ui.Escribir("  🟢 MISIÓN COMPLETADA", ConsoleColor.Green);
            else Ui.Escribir("  Estado: EN CURSO", ConsoleColor.Yellow);
            Ui.Pausa();
        }
    }

    public static class Encuentros
    {
        public static ResultadoEncuentro Iniciar(Nexus nexus, Personaje entidad)
        {
            Cadete cadete = nexus.Cadete;
            Ui.Titulo("👾 ENTIDAD DETECTADA", ConsoleColor.Magenta);
            Console.WriteLine("Tipo:   " + entidad.Tipo);
            Console.WriteLine("Nombre: " + entidad.Nombre);
            Thread.Sleep(800);
            entidad.AlEncontrarse(cadete);
            if (cadete.Energia <= 0) Juego.GameOver(nexus, entidad.Nombre + " acabó con tu energía.");
            Thread.Sleep(1200);
            while (true)
            {
                MostrarEstado(cadete, entidad);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n  [1] Atacar");
                Console.WriteLine("  [2] Escapar");
                Console.WriteLine("  [3] Utilizar un objeto");
                Console.WriteLine("  [4] Analizar la entidad");
                Console.Write("\nElige una acción: ");
                int opcion;
                if (int.TryParse(Console.ReadLine(), out opcion) == false || opcion < 1 || opcion > 4)
                {
                    Ui.Escribir("\n✖ Opción inválida. Elige un número del 1 al 4.", ConsoleColor.Red);
                    Thread.Sleep(1200);
                    continue;
                }
                bool turnoConsumido = false;
                Console.WriteLine();
                if (opcion == 1)
                {
                    turnoConsumido = Atacar(cadete, entidad);
                    if (entidad.EstaVivo == false) return Victoria(nexus, entidad);
                }
                else if (opcion == 2)
                {
                    bool escapo = Escapar(cadete, entidad);
                    if (escapo) return ResultadoEncuentro.Escape;
                    turnoConsumido = true;
                }
                else if (opcion == 3) turnoConsumido = nexus.UsarObjetoMenu(entidad, true);
                else Analizar(cadete, entidad);
                if (turnoConsumido && entidad.EstaVivo)
                {
                    Thread.Sleep(600);
                    entidad.Turno(cadete);
                    if (cadete.Energia <= 0) Juego.GameOver(nexus, entidad.Nombre + " te ha derrotado.");
                }
                Ui.Pausa();
            }
        }

        private static bool Atacar(Cadete cadete, Personaje entidad)
        {
            Ui.Escribir("⚔ Atacas a " + entidad.Nombre + "...", ConsoleColor.Green);
            double real = cadete.Atacar(entidad);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Daño causado: -" + real + "   (" + entidad.Nombre + ": " + entidad.Energia + "/" + entidad.EnergiaMaxima + ")");
            if (cadete.UltimoAtaqueConModulo) Ui.Escribir("🔫 Módulo de ataque: +10 de daño.", ConsoleColor.Yellow);
            if (cadete.UltimoAtaqueAPuntoDebil) Ui.Escribir("🎯 ¡Golpe al punto débil! (+50% de daño)", ConsoleColor.Yellow);
            return true;
        }

        private static bool Escapar(Cadete cadete, Personaje entidad)
        {
            Ui.Escribir("🏃 Intentas escapar de " + entidad.Nombre + "...", ConsoleColor.Yellow);
            Thread.Sleep(800);
            if (cadete.IntentarEscapar(entidad))
            {
                double antes = cadete.Energia;
                cadete.PerderEnergiaNoLetal(5);
                Ui.Escribir("✓ ¡Lograste escapar!", ConsoleColor.Green);
                Ui.Escribir("Energía: " + antes + " → " + cadete.Energia + "   (-5 por la huida)", ConsoleColor.Yellow);
                Ui.Pausa();
                return true;
            }
            Ui.Escribir("✖ No logras escapar. ¡La entidad te corta el paso!", ConsoleColor.Red);
            return false;
        }

        private static void Analizar(Cadete cadete, Personaje entidad)
        {
            double perdida = cadete.PerderEnergiaNoLetal(2);
            Ui.Escribir("🔍 Escaneando entidad... (-" + perdida + " de energía)", ConsoleColor.Cyan);
            Thread.Sleep(700);
            entidad.MostrarFicha();
        }

        private static ResultadoEncuentro Victoria(Nexus nexus, Personaje entidad)
        {
            bool esJefe = false;
            int xp = 20;
            if (entidad is Enemigo)
            {
                Enemigo enemigo = (Enemigo)entidad;
                esJefe = enemigo.EsJefe;
            }
            if (entidad is Robot) xp = 15;
            if (esJefe) xp = 60;
            Console.WriteLine();
            Ui.Escribir("✔ " + entidad.Nombre + " HA SIDO DERROTADO.", ConsoleColor.Green);
            nexus.GanarXp(xp);
            nexus.EntidadesDerrotadas++;
            if (esJefe || Juego.Azar.Next(100) < 50)
            {
                Objeto botin = Objeto.Aleatorio();
                Ui.Escribir("\n🎁 La entidad dejó caer: " + botin.Icono + " " + botin.Nombre, ConsoleColor.Magenta);
                nexus.RecogerObjeto(botin);
            }
            Ui.Pausa();
            return ResultadoEncuentro.Victoria;
        }

        private static void MostrarEstado(Cadete cadete, Personaje entidad)
        {
            Console.Clear();
            Ui.Escribir("══════════════════ ⚔  ENCUENTRO  ⚔ ══════════════════", ConsoleColor.Red);
            Console.WriteLine("                                        \r\n                                        \r\n                    @@@%%##%@           \r\n                   @@@%%%###%@          \r\n                   @%%%##@@@@@@         \r\n                   =- .-:.%@@@@         \r\n                         :%@@@@         \r\n                  @@%*+=#@@@@+**@       \r\n         @@  @%%*-=====*@#+#%%*:%       \r\n     @@****:+*@@%+=+---=+%*#%***#       \r\n      @%**#***#@%#+*###%#@#%@*++*@      \r\n     @@@@@%*%@@@@#*#%%%*****#*%%##@     \r\n        %=+**%@@%*%@@@@@++..**@@##      \r\n             @@@@  %%%%%@@%@@@@@        \r\n                 *+==+##+--=*%          \r\n                #===+*#@+=-=*#          \r\n                %#*#@   %*+*#%@         \r\n                @@@@@    %#%%@@@        \r\n               @@@@%#      @@@@@-*      \r\n             @@@@@@@@@@@@@@@@@@@@@@     \r\n                                       ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("CADETE " + cadete.Nombre);
            Console.WriteLine("  Energía     [" + Ui.Barra(cadete.Energia, cadete.EnergiaMaxima, 20) + "] " + cadete.Energia + "/" + cadete.EnergiaMaxima);
            Console.WriteLine("  Estabilidad " + cadete.Estabilidad + "/100     Escudo: " + cadete.Escudo + "     Módulo: " + cadete.CargasModulo + " cargas");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine(entidad.Tipo + ": " + entidad.Nombre);
            Console.WriteLine("  Energía     [" + Ui.Barra(entidad.Energia, entidad.EnergiaMaxima, 20) + "] " + entidad.Energia + "/" + entidad.EnergiaMaxima);
            if (entidad is Robot)
            {
                Robot robot = (Robot)entidad;
                if (robot.DefensaActiva) Console.WriteLine("  🛡️ DEFENSA ACTIVA");
            }
            if (entidad is Enemigo)
            {
                Enemigo enemigo = (Enemigo)entidad;
                if (enemigo.Enfurecido) Console.WriteLine("  EN FURIA");
            }
            if (entidad.PuntoDebilExpuesto) Console.WriteLine("   PUNTO DÉBIL EXPUESTO");
            Ui.Escribir("══════════════════════════════════════════════════════", ConsoleColor.Red);
        }
    }

    public class Zona
    {
        public const int Alto = 20;
        public const int Ancho = 30;
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public ConsoleColor ColorBorde { get; set; }
        public string[] Filas { get; set; }
        public string[] Leyenda { get; set; }
        public string IconoCoordenadas { get; set; }
        public int Cartografia { get; set; }
        public string Estado { get; set; }
        public int Lecturas { get; set; }
        public int Senales { get; set; }
        public int ProbEvento { get; set; }
        public char[,] Mapa { get; private set; }
        public int Fila { get; set; }
        public int Columna { get; set; }

        public void Preparar()
        {
            if (Mapa != null) return;
            Mapa = new char[Alto, Ancho];
            for (int fila = 0; fila < Alto; fila++)
            {
                for (int columna = 0; columna < Ancho; columna++)
                {
                    if (columna < Filas[fila].Length) Mapa[fila, columna] = Filas[fila][columna];
                    else Mapa[fila, columna] = '.';
                    if (Mapa[fila, columna] == '⌂')
                    {
                        Fila = fila;
                        Columna = columna;
                    }
                }
            }
        }

        public ConsoleColor ColorTerreno(char terreno)
        {
            if (terreno == '^') return ConsoleColor.DarkGray;
            if (terreno == '~') return ConsoleColor.Blue;
            if (terreno == '#') return ConsoleColor.Green;
            if (terreno == '-') return ConsoleColor.Yellow;
            if (terreno == '⌂') return ConsoleColor.White;
            if (terreno == '◆') return ConsoleColor.Cyan;
            if (terreno == '⚠') return ConsoleColor.Red;
            if (terreno == '●') return ConsoleColor.Magenta;
            if (terreno == '?') return ConsoleColor.DarkGray;
            if (Nombre == "IRIS-01")
            {
                if (terreno == '✦') return ConsoleColor.Yellow;
                if (terreno == '▓') return ConsoleColor.DarkGreen;
                if (terreno == '◉') return ConsoleColor.Cyan;
                if (terreno == '◇') return ConsoleColor.White;
            }
            else if (Nombre == "IRIS-02")
            {
                if (terreno == '≈') return ConsoleColor.Cyan;
                if (terreno == '◉') return ConsoleColor.White;
                if (terreno == '◇') return ConsoleColor.Yellow;
                if (terreno == '✦') return ConsoleColor.Magenta;
            }
            else if (Nombre == "IRIS-03")
            {
                if (terreno == '☢') return ConsoleColor.Yellow;
                if (terreno == '▓') return ConsoleColor.DarkRed;
                if (terreno == '✦') return ConsoleColor.Red;
                if (terreno == '◉') return ConsoleColor.Magenta;
                if (terreno == '◇') return ConsoleColor.White;
            }
            return ConsoleColor.Gray;
        }

        public static Zona[] Todas()
        {
            Zona iris01 = new Zona();
            iris01.Nombre = "IRIS-01";
            iris01.Titulo = "⛰ SECTOR MONTAÑOSO ⛰";
            iris01.ColorBorde = ConsoleColor.DarkGray;
            iris01.IconoCoordenadas = "⛰";
            iris01.Cartografia = 78;
            iris01.Estado = "ESTABLE";
            iris01.Lecturas = 1;
            iris01.Senales = 3;
            iris01.ProbEvento = 18;
            iris01.Filas = new string[] {
                "^^^^^^^^^^......####........??",
                "^^^^^◆^^^^......####........??",
                "^^^^^^^^^~~~~....##●#.......??",
                "........~~~~....####..------.?",
                ".........~~...........--⌂---.?",
                ".........~~...........--......",
                ".........~............-..⚠...?",
                ".........~............-.......",
                ".........~~~~~........-.......",
                ".........~~~~~................",
                "...............✦..............",
                "..............▓▓▓.............",
                "..............▓◉▓.............",
                "..............▓▓▓.............",
                "..............................",
                "..............................",
                "..............◇...............",
                "..............................",
                "..............................",
                ".............................."
            };
            iris01.Leyenda = new string[] {
                " ^ Montañas       ~ Río             # Bosque",
                " - Camino         . Territorio      ⌂ Base",
                " ◆ Recurso        ⚠ Anomalía        ● Punto de interés",
                " ? Desconocido    ✦ Zona especial   ◇ Señal",
                " ▓ Estructura     ◉ Punto activo    @ Tú (Cadete)"
            };

            Zona iris02 = new Zona();
            iris02.Nombre = "IRIS-02";
            iris02.Titulo = "≈ SECTOR ACUATICO ≈";
            iris02.ColorBorde = ConsoleColor.DarkBlue;
            iris02.IconoCoordenadas = "≈";
            iris02.Cartografia = 65;
            iris02.Estado = "INESTABLE";
            iris02.Lecturas = 2;
            iris02.Senales = 4;
            iris02.ProbEvento = 25;
            iris02.Filas = new string[] {
                "~~~~~~~~~~..................??",
                "~~~~~~~~............########??",
                "~~~~~~~...^^^^..............??",
                "~~~~~.....^◆^^..............??",
                "~~~~......^^................?.",
                "~~~....---------.............?",
                "~~.........-⌂--.............?.",
                "~..........----...⚠.........?.",
                ".................-----.......?",
                ".....................●........",
                "..............≈≈≈.............",
                ".............≈◉≈≈............",
                "..............≈≈≈.............",
                "..............................",
                "............◇.................",
                "..............................",
                "..................✦...........",
                "..............................",
                "..............................",
                ".............................."
            };
            iris02.Leyenda = new string[] {
                " ~ Agua            ^ Montañas        # Bosque",
                " - Camino          . Territorio      ⌂ Base",
                " ◆ Recurso         ⚠ Anomalía        ● Punto de interés",
                " ? Desconocido     ≈ Corriente       ◉ Señal acuática",
                " ◇ Señal           ✦ Zona especial   @ Tú (Cadete)"
            };

            Zona iris03 = new Zona();
            iris03.Nombre = "IRIS-03";
            iris03.Titulo = "☢ SECTOR ANOMALO ☢";
            iris03.ColorBorde = ConsoleColor.DarkRed;
            iris03.IconoCoordenadas = "☢";
            iris03.Cartografia = 71;
            iris03.Estado = "CRITICO";
            iris03.Lecturas = 5;
            iris03.Senales = 7;
            iris03.ProbEvento = 32;
            iris03.Filas = new string[] {
                "~~~~~~~~~~~~................??",
                "~~~~~~~~~~~....^^^^^........??",
                "~~~~~~~~...^^^^◆^^^.........??",
                "~~~~~........^^^....####....??",
                "~~~~................#●##....?.",
                "~~~....--------⌂---.####....?.",
                "~~...............-...........?",
                "~................------⚠....?.",
                "..............☢...............",
                ".............▓▓▓..............",
                "............▓☢▓▓..............",
                ".............▓▓▓..............",
                "................✦.............",
                "..............◉...............",
                "..............................",
                "...........◇..................",
                "..............................",
                ".................⚠............",
                "..............................",
                ".............................."
            };
            iris03.Leyenda = new string[] {
                " ~ Agua            ^ Montañas        # Bosque",
                " - Camino          . Territorio      ⌂ Base",
                " ◆ Recurso         ⚠ Anomalía        ● Punto de interés",
                " ? Desconocido     ☢ Zona peligrosa  ▓ Estructura",
                " ✦ Alta actividad  ◉ Señal activa    ◇ Señal desconocida",
                " @ Tú (Cadete)"
            };
            return new Zona[] { iris01, iris02, iris03 };
        }
    }

    public static class Exploracion
    {
        private static Zona[] zonas = Zona.Todas();
        private static string ultimoAviso = "";
        private static ConsoleColor colorAviso = ConsoleColor.Gray;

        private static string[] informacion = new string[] {
            "Registro NEXUS-07: la anomalía no es un lugar, es una herida en la realidad.",
            "Datos recuperados: el núcleo energético se detecta como un destello ◆ en el mapa.",
            "Bitácora: el punto de extracción ⌂ es el único sector estable de la zona.",
            "Alerta: los robots activan un escudo que reduce tu daño. Un ANALIZADOR lo anula.",
            "Registro: las entidades del vacío se vuelven más violentas cuando están a punto de caer.",
            "Dato: las zonas ☢ drenan energía y estabilidad. Evítalas si estás débil.",
            "Bitácora del cadete anterior: 'si escuchas pasos donde no hay nadie, usa un escudo'.",
            "Fragmento: las montañas ^ y el agua ~ agotan tu energía al cruzarlas."
        };

        private static void Aviso(string texto, ConsoleColor color)
        {
            ultimoAviso = texto;
            colorAviso = color;
        }

        public static void MenuRealidades(Nexus nexus)
        {
            int seleccion = 0;
            while (seleccion != 4)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("╔════════════════════════════════════════════════════╗");
                Console.WriteLine("║                  NEXUS // REALIDADES               ║");
                Console.WriteLine("╠════════════════════════════════════════════════════╣");
                Console.WriteLine("║                                                    ║");
                Console.WriteLine("║   [1] IRIS-01                                      ║");
                Console.WriteLine("║   [2] IRIS-02                                      ║");
                Console.WriteLine("║   [3] IRIS-03                                      ║");
                Console.WriteLine("║                                                    ║");
                Console.WriteLine("║   [4] Regresar                                     ║");
                Console.WriteLine("║                                                    ║");
                Console.WriteLine("╚════════════════════════════════════════════════════╝");
                Console.Write("\nSeleccione una realidad: ");
                if (int.TryParse(Console.ReadLine(), out seleccion) == false)
                {
                    Console.WriteLine("\nOpcion no valida.");
                    Thread.Sleep(1000);
                    continue;
                }
                if (seleccion >= 1 && seleccion <= 3) Explorar(nexus, zonas[seleccion - 1]);
                else if (seleccion != 4)
                {
                    Console.WriteLine("\nOpcion no valida.");
                    Thread.Sleep(1000);
                }
            }
        }

        private static void Explorar(Nexus nexus, Zona zona)
        {
            zona.Preparar();
            MostrarBriefing(nexus, zona);
            Aviso("Exploración iniciada. Usa W A S D para moverte.", ConsoleColor.Cyan);
            if (zona.Mapa[zona.Fila, zona.Columna] == '⌂') TileBase(nexus);
            bool salir = false;
            while (salir == false)
            {
                Dibujar(nexus, zona);
                ConsoleKeyInfo tecla = Console.ReadKey(true);
                int nuevaFila = zona.Fila;
                int nuevaColumna = zona.Columna;
                bool movimiento = false;
                switch (tecla.Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        nuevaFila--;
                        movimiento = true;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        nuevaFila++;
                        movimiento = true;
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        nuevaColumna--;
                        movimiento = true;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        nuevaColumna++;
                        movimiento = true;
                        break;
                    case ConsoleKey.I:
                        nexus.MostrarInventario();
                        Aviso("Regresas a la exploración.", ConsoleColor.Gray);
                        break;
                    case ConsoleKey.U:
                        UsarObjetoRapido(nexus);
                        break;
                    case ConsoleKey.F:
                        Aviso("⚔ No hay ninguna entidad cerca. No tienes a quién atacar.", ConsoleColor.Yellow);
                        break;
                    case ConsoleKey.Z:
                        Aviso("🔍 No hay ninguna entidad para analizar.", ConsoleColor.Yellow);
                        break;
                    case ConsoleKey.M:
                        nexus.MostrarMisiones();
                        Aviso("Regresas a la exploración.", ConsoleColor.Gray);
                        break;
                    case ConsoleKey.L:
                        MostrarLeyenda(zona);
                        Aviso("Regresas a la exploración.", ConsoleColor.Gray);
                        break;
                    case ConsoleKey.Q:
                        salir = ConfirmarSalida();
                        if (salir == false) Aviso("Continúas explorando.", ConsoleColor.Gray);
                        break;
                    default:
                        Aviso("✖ Tecla no válida. Usa W/A/S/D para moverte (o I, U, F, Z, M, L, Q).", ConsoleColor.Red);
                        break;
                }
                if (movimiento) Mover(nexus, zona, nuevaFila, nuevaColumna);
            }
            Console.Clear();
            Ui.Marco(ConsoleColor.Cyan, "◈ TRANSMISION NEXUS FINALIZADA ◈", "", "Presione ENTER para regresar...");
            Console.ReadLine();
        }

        private static bool ConfirmarSalida()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n¿Salir de la realidad? (S/N): ");
            ConsoleKeyInfo tecla = Console.ReadKey(true);
            return tecla.Key == ConsoleKey.S;
        }

        private static void UsarObjetoRapido(Nexus nexus)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', Ui.Ancho) + "╗");
            Console.WriteLine("║" + Ui.Centrar("NEXUS // UTILIZAR OBJETO", Ui.Ancho) + "║");
            Console.WriteLine("╠" + new string('═', Ui.Ancho) + "╣");
            nexus.Inventario.MostrarLista();
            nexus.UsarObjetoMenu(null, false);
            Ui.Pausa();
            Aviso("Regresas a la exploración.", ConsoleColor.Gray);
        }

        private static int CostoTerreno(char terreno)
        {
            if (terreno == '^') return 3;
            if (terreno == '~') return 2;
            if (terreno == '#') return 1;
            if (terreno == '≈') return 1;
            return 0;
        }

        private static string NombreTerreno(char terreno)
        {
            if (terreno == '^') return "Montaña";
            if (terreno == '~') return "Agua";
            if (terreno == '#') return "Bosque";
            if (terreno == '-') return "Camino";
            if (terreno == '≈') return "Corriente";
            if (terreno == '⌂') return "Base";
            if (terreno == '▓') return "Estructura";
            if (terreno == '☢') return "Zona peligrosa";
            return "Territorio";
        }

        private static void Mover(Nexus nexus, Zona zona, int nuevaFila, int nuevaColumna)
        {
            Cadete cadete = nexus.Cadete;
            if (nuevaFila < 0 || nuevaFila >= Zona.Alto || nuevaColumna < 0 || nuevaColumna >= Zona.Ancho)
            {
                Aviso("⛔ LÍMITE DEL MAPA: no puedes salir de la zona de exploración.", ConsoleColor.Red);
                return;
            }
            char terreno = zona.Mapa[nuevaFila, nuevaColumna];
            int costo = CostoTerreno(terreno);
            if (costo > 0 && cadete.Energia <= costo)
            {
                Aviso("⚡ Energía insuficiente para cruzar: " + NombreTerreno(terreno).ToLower() + " (cuesta " + costo + "). Usa una célula o recupera energía.", ConsoleColor.Red);
                return;
            }
            zona.Fila = nuevaFila;
            zona.Columna = nuevaColumna;
            if (costo > 0)
            {
                cadete.PerderEnergia(costo);
                Aviso("Avanzas por " + NombreTerreno(terreno).ToLower() + ": -" + costo + " de energía.", ConsoleColor.DarkYellow);
            }
            else Aviso("Te desplazas a [X: " + nuevaColumna + ", Y: " + nuevaFila + "].", ConsoleColor.Gray);
            ResolverCasilla(nexus, zona, terreno);
        }

        private static void ResolverCasilla(Nexus nexus, Zona zona, char terreno)
        {
            int f = zona.Fila;
            int c = zona.Columna;
            if (terreno == '⚠')
            {
                TileAnomalia(nexus);
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '◆')
            {
                if (TileRecurso(nexus)) zona.Mapa[f, c] = '.';
            }
            else if (terreno == '⌂') TileBase(nexus);
            else if (terreno == '●')
            {
                TileInformacion(nexus, "PUNTO DE INTERÉS", "Has llegado a un punto de interés registrado por NEXUS.");
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '◉')
            {
                TileInformacion(nexus, "SEÑAL ACTIVA", "Una terminal de datos responde a tu presencia.");
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '◇')
            {
                TileSenal(nexus);
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '✦')
            {
                TileEspecial(nexus);
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '?')
            {
                EventoAleatorio(nexus, 100);
                zona.Mapa[f, c] = '.';
            }
            else if (terreno == '☢') TileRadiacion(nexus);
            else if (terreno == '▓')
            {
                if (TileRuinas(nexus)) zona.Mapa[f, c] = '.';
            }
            else
            {
                int probabilidad = zona.ProbEvento;
                if (terreno == '-') probabilidad = probabilidad / 2;
                EventoAleatorio(nexus, probabilidad);
            }
        }

        private static void EventoAleatorio(Nexus nexus, int probabilidad)
        {
            if (Juego.Azar.Next(100) >= probabilidad) return;
            int evento = Juego.Azar.Next(100);
            if (evento < 25) EventoObjeto(nexus);
            else if (evento < 45) EventoEntidad(nexus, Entidades.CrearAleatoria(nexus.Nivel));
            else if (evento < 60) EventoPerdidaEnergia(nexus);
            else if (evento < 75) EventoSistemaDanado(nexus);
            else if (evento < 90) EventoAnomalia(nexus);
            else EventoInformacion(nexus);
        }

        private static void EventoObjeto(Nexus nexus)
        {
            Ui.Titulo("🎁 OBJETO ENCONTRADO", ConsoleColor.Green);
            Objeto objeto = Objeto.Aleatorio();
            Console.WriteLine("Entre los restos de la realidad encuentras algo útil:");
            Console.WriteLine();
            Ui.Escribir("  " + objeto.Icono + " " + objeto.Nombre + "  -  " + objeto.Descripcion, ConsoleColor.Yellow);
            if (nexus.RecogerObjeto(objeto)) Aviso("🎁 Recogiste " + objeto.Nombre + ".", ConsoleColor.Green);
            else Aviso("No te llevaste " + objeto.Nombre + ".", ConsoleColor.DarkGray);
            Ui.Pausa();
        }

        private static void EventoEntidad(Nexus nexus, Personaje entidad)
        {
            ResultadoEncuentro resultado = Encuentros.Iniciar(nexus, entidad);
            if (resultado == ResultadoEncuentro.Victoria) Aviso("Has despejado la zona.", ConsoleColor.Green);
            else Aviso("Escapaste del encuentro. La zona sigue siendo peligrosa.", ConsoleColor.Yellow);
        }

        private static void EventoPerdidaEnergia(Nexus nexus)
        {
            Ui.Titulo("🔋 PÉRDIDA DE ENERGÍA", ConsoleColor.Yellow);
            Console.WriteLine("Una fuga en tu núcleo energético drena tus reservas.");
            Console.WriteLine();
            double antes = nexus.Cadete.Energia;
            double perdida = nexus.Cadete.PerderEnergiaNoLetal(Juego.Azar.Next(5, 13));
            Ui.Escribir("Energía: " + antes + " → " + nexus.Cadete.Energia + "   (-" + perdida + ")", ConsoleColor.Red);
            if (nexus.Cadete.Energia < 25) Ui.Escribir("\n⚠ Energía crítica. Usa una célula o recupera energía desde el menú principal.", ConsoleColor.Yellow);
            Aviso("🔋 Perdiste " + perdida + " de energía.", ConsoleColor.Red);
            Ui.Pausa();
        }

        private static void EventoSistemaDanado(Nexus nexus)
        {
            Ui.Titulo("🚨 SISTEMA DAÑADO", ConsoleColor.Red);
            Console.WriteLine("Una descarga de la realidad golpea los sistemas de tu traje.");
            Console.WriteLine();
            double antes = nexus.Cadete.Estabilidad;
            nexus.Cadete.PerderEstabilidad(Juego.Azar.Next(8, 16));
            Ui.Escribir("Estabilidad: " + antes + " → " + nexus.Cadete.Estabilidad, ConsoleColor.Red);
            List<Objeto> candidatos = new List<Objeto>();
            foreach (Objeto objeto in nexus.Inventario.Lista)
            {
                if (objeto.Tipo != TipoObjeto.NucleoResonancia) candidatos.Add(objeto);
            }
            if (candidatos.Count > 0 && Juego.Azar.Next(100) < 35)
            {
                Objeto roto = candidatos[Juego.Azar.Next(candidatos.Count)];
                nexus.Inventario.Eliminar(roto);
                Ui.Escribir("💥 " + roto.Nombre + " quedó inutilizable por el impacto.", ConsoleColor.Red);
            }
            Aviso("🚨 Sistema dañado: tu estabilidad bajó.", ConsoleColor.Red);
            Ui.Pausa();
        }

        private static void EventoAnomalia(Nexus nexus)
        {
            Ui.Titulo("⚡ ANOMALÍA DETECTADA", ConsoleColor.Yellow);
            Console.WriteLine("Tus sensores registran una distorsión en la realidad cercana.");
            Console.WriteLine();
            nexus.AgregarAnomalia();
            nexus.Cadete.PerderEstabilidad(3);
            Ui.Escribir("Anomalías registradas: " + nexus.Anomalias + "   (-3 estabilidad)", ConsoleColor.Yellow);
            Ui.Escribir("Usa 'Analizar anomalía' en el menú principal para estudiarla.", ConsoleColor.Gray);
            Aviso("⚡ Anomalía registrada (" + nexus.Anomalias + ").", ConsoleColor.Yellow);
            Ui.Pausa();
        }

        private static void EventoInformacion(Nexus nexus)
        {
            Ui.Titulo("📡 INFORMACIÓN ENCONTRADA", ConsoleColor.Cyan);
            Console.WriteLine("Captas un fragmento de datos de NEXUS:");
            Console.WriteLine();
            Ui.Escribir("» " + informacion[Juego.Azar.Next(informacion.Length)], ConsoleColor.White);
            Console.WriteLine();
            nexus.GanarXp(5);
            Aviso("📡 Información archivada.", ConsoleColor.Cyan);
            Ui.Pausa();
        }

        private static void TileAnomalia(Nexus nexus)
        {
            Ui.Titulo("⚡ ANOMALÍA DETECTADA", ConsoleColor.Yellow);
            Console.WriteLine("Has llegado al epicentro de una grieta en la realidad.");
            Console.WriteLine("Tus sensores enloquecen y el aire vibra a tu alrededor.");
            Console.WriteLine();
            nexus.AgregarAnomalia();
            nexus.Cadete.PerderEstabilidad(5);
            Ui.Escribir("Anomalías registradas: " + nexus.Anomalias + "   (-5 estabilidad)", ConsoleColor.Yellow);
            if (nexus.NotificarMision("anomalia"))
            {
                if (nexus.Inventario.Tiene(TipoObjeto.NucleoResonancia) && nexus.NotificarMision("nucleo"))
                {
                    Ui.Pausa();
                    DespertarGuardian(nexus);
                    return;
                }
            }
            Aviso("⚡ Zona de anomalía explorada.", ConsoleColor.Yellow);
            Ui.Pausa();
        }

        private static bool TileRecurso(Nexus nexus)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Arte.Exploracion);
            Console.WriteLine("╔" + new string('═', Ui.Ancho) + "╗");
            Console.WriteLine("║" + Ui.Ajustar("  ◆ Se ha detectado una lectura de materia desconocida.", Ui.Ancho) + "║");
            Console.WriteLine("║" + new string(' ', Ui.Ancho) + "║");
            Console.WriteLine("║" + Ui.Ajustar("  [1] Investigar el recurso", Ui.Ancho) + "║");
            Console.WriteLine("║" + Ui.Ajustar("  [2] Ignorar y continuar", Ui.Ancho) + "║");
            Console.WriteLine("╚" + new string('═', Ui.Ancho) + "╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nSeleccione una acción: ");
            int opcion;
            bool recogido = false;
            if (int.TryParse(Console.ReadLine(), out opcion) == false)
            {
                Ui.Escribir("\nOpción no válida. La exploración continúa.", ConsoleColor.Red);
                Thread.Sleep(1500);
            }
            else if (opcion == 1)
            {
                Ui.Escribir("\n◆ ANALIZANDO RECURSO...", ConsoleColor.Yellow);
                Thread.Sleep(1000);
                Console.WriteLine("Lectura completada.");
                Console.WriteLine("Se ha encontrado un NÚCLEO DE RESONANCIA.");
                Console.WriteLine("Este objeto puede ser utilizado posteriormente.");
                recogido = nexus.RecogerObjeto(Objeto.Crear(TipoObjeto.NucleoResonancia));
                if (recogido)
                {
                    Ui.Escribir("\n✓ RECURSO OBTENIDO", ConsoleColor.Green);
                    Thread.Sleep(1200);
                    if (nexus.NotificarMision("nucleo"))
                    {
                        Ui.Pausa();
                        DespertarGuardian(nexus);
                        return true;
                    }
                    Objetivo pendiente = nexus.MisionPrincipal.Pendiente;
                    if (pendiente != null && pendiente.Clave == "anomalia") Ui.Escribir("\nNEXUS: el núcleo está en tu poder, pero aún debes llegar a la zona de anomalía (⚠).", ConsoleColor.Cyan);
                }
            }
            else if (opcion == 2)
            {
                Ui.Escribir("\nHas decidido ignorar el recurso.", ConsoleColor.DarkGray);
                Thread.Sleep(1000);
            }
            else
            {
                Ui.Escribir("\nOpción no válida.", ConsoleColor.Red);
                Console.WriteLine("La exploración continúa...");
                Thread.Sleep(1500);
            }
            if (recogido) Aviso("◆ Recurso obtenido.", ConsoleColor.Green);
            Ui.Pausa();
            return recogido;
        }

        private static void DespertarGuardian(Nexus nexus)
        {
            Ui.Titulo("🚨 ¡EL NÚCLEO ESTABA PROTEGIDO!", ConsoleColor.Red);
            Console.WriteLine("La anomalía vibra violentamente...");
            Console.WriteLine("Una entidad colosal emerge de la grieta para recuperar lo que es suyo.");
            Thread.Sleep(1800);
            ResultadoEncuentro resultado = Encuentros.Iniciar(nexus, Entidades.CrearGuardian(nexus.Nivel));
            nexus.NotificarMision("entidad");
            if (resultado == ResultadoEncuentro.Victoria) Ui.Escribir("\nEl guardián se desintegra. La zona vuelve a la calma.", ConsoleColor.Green);
            else Ui.Escribir("\nHuyes del guardián, pero sigues con vida.", ConsoleColor.Yellow);
            Ui.Escribir("Regresa al punto de extracción (⌂) para completar la misión.", ConsoleColor.Cyan);
            Aviso("▶ Regresa al punto de extracción (⌂).", ConsoleColor.Cyan);
            Ui.Pausa();
        }

        private static void TileBase(Nexus nexus)
        {
            Mision mision = nexus.MisionPrincipal;
            Objetivo pendiente = mision.Pendiente;
            if (mision.Completada)
            {
                Aviso("⌂ Punto de extracción seguro. La misión ya fue completada.", ConsoleColor.Green);
                return;
            }
            if (pendiente.Clave == "extraccion")
            {
                Ui.Titulo("⌂ PUNTO DE EXTRACCIÓN", ConsoleColor.White);
                Console.WriteLine("Entregas el núcleo y la señal de extracción se activa...");
                Thread.Sleep(1000);
                nexus.NotificarMision("extraccion");
                Aviso("🟢 Misión completada.", ConsoleColor.Green);
                return;
            }
            Aviso("⌂ Punto de extracción. Objetivo pendiente: " + pendiente.Descripcion, ConsoleColor.Cyan);
        }

        private static void TileInformacion(Nexus nexus, string titulo, string introduccion)
        {
            Ui.Titulo("📡 " + titulo, ConsoleColor.Cyan);
            Console.WriteLine(introduccion);
            Console.WriteLine();
            Ui.Escribir("» " + informacion[Juego.Azar.Next(informacion.Length)], ConsoleColor.White);
            Console.WriteLine();
            nexus.GanarXp(10);
            if (Juego.Azar.Next(100) < 35)
            {
                Objeto objeto = Objeto.Aleatorio();
                Ui.Escribir("\n🎁 Además recuperas: " + objeto.Icono + " " + objeto.Nombre, ConsoleColor.Magenta);
                nexus.RecogerObjeto(objeto);
            }
            Aviso("📡 Datos archivados (+10 XP).", ConsoleColor.Cyan);
            Ui.Pausa();
        }

        private static void TileSenal(Nexus nexus)
        {
            Ui.Titulo("📡 SEÑAL DESCONOCIDA", ConsoleColor.Yellow);
            Console.WriteLine("Interceptas una señal de seguridad de NEXUS.");
            Console.WriteLine("Un autómata acude a tu posición...");
            Thread.Sleep(1200);
            EventoEntidad(nexus, Entidades.CrearRobot(nexus.Nivel));
        }

        private static void TileEspecial(Nexus nexus)
        {
            Ui.Titulo("✦ ZONA ESPECIAL", ConsoleColor.Magenta);
            Console.WriteLine("Un pulso de resonancia atraviesa tu traje.");
            Console.WriteLine();
            double energiaAntes = nexus.Cadete.Energia;
            double estabilidadAntes = nexus.Cadete.Estabilidad;
            nexus.Cadete.AgregarEnergia(15);
            nexus.Cadete.AumentarEstabilidad(10);
            Ui.Escribir("Energía: " + energiaAntes + " → " + nexus.Cadete.Energia, ConsoleColor.Green);
            Ui.Escribir("Estabilidad: " + estabilidadAntes + " → " + nexus.Cadete.Estabilidad, ConsoleColor.Green);
            Aviso("✦ La zona especial te recarga.", ConsoleColor.Magenta);
            if (Juego.Azar.Next(100) < 30)
            {
                Ui.Escribir("\n👾 El pulso ha atraído a una entidad...", ConsoleColor.Red);
                Ui.Pausa();
                EventoEntidad(nexus, Entidades.CrearEnemigo(nexus.Nivel));
            }
            else Ui.Pausa();
        }

        private static void TileRadiacion(Nexus nexus)
        {
            double perdida = nexus.Cadete.PerderEnergiaNoLetal(8);
            nexus.Cadete.PerderEstabilidad(5);
            Aviso("☢ ZONA PELIGROSA: radiación (-" + perdida + " energía, -5 estabilidad).", ConsoleColor.Red);
        }

        private static bool TileRuinas(Nexus nexus)
        {
            if (Juego.Azar.Next(100) < 25)
            {
                EventoObjeto(nexus);
                return true;
            }
            Aviso("▓ Ruinas de una estructura antigua. No encuentras nada útil.", ConsoleColor.DarkGray);
            return false;
        }

        private static void MostrarBriefing(Nexus nexus, Zona zona)
        {
            Console.Clear();
            Ui.Marco(ConsoleColor.Gray, "NEXUS // CARTOGRAFIA", zona.Nombre, zona.Titulo);
            Console.WriteLine();
            ImprimirLeyenda(zona);
            ImprimirInformacion(zona);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Arte.Exploracion);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Controles:  [W/A/S/D] Mover   [I] Inventario   [U] Usar objeto");
            Console.WriteLine("              [F] Atacar   [Z] Analizar   [M] Misión   [L] Leyenda   [Q] Salir");
            Console.WriteLine();
            nexus.Cadete.Explorar();
            Ui.Pausa("Presione ENTER para comenzar la exploración...");
        }

        private static void ImprimirLeyenda(Zona zona)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("╔" + new string('═', Ui.Ancho) + "╗");
            Console.WriteLine("║" + Ui.Centrar("LEYENDA", Ui.Ancho) + "║");
            Console.WriteLine("╠" + new string('═', Ui.Ancho) + "╣");
            foreach (string linea in zona.Leyenda) Console.WriteLine("║" + Ui.Ajustar(linea, Ui.Ancho) + "║");
            Console.WriteLine("╚" + new string('═', Ui.Ancho) + "╝");
        }

        private static void ImprimirInformacion(Zona zona)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine(zona.IconoCoordenadas + " Coordenadas: [X: " + zona.Columna.ToString("00") + ", Y: " + zona.Fila.ToString("00") + "]");
            Console.WriteLine("◈ Nivel de cartografía: " + zona.Cartografia + "%");
            Console.WriteLine("◈ Estado territorial: " + zona.Estado);
            Console.WriteLine("◈ Lecturas anómalas: " + zona.Lecturas.ToString("00"));
            Console.WriteLine("◈ Señales detectadas: " + zona.Senales.ToString("00"));
        }

        private static void MostrarLeyenda(Zona zona)
        {
            Console.Clear();
            ImprimirLeyenda(zona);
            ImprimirInformacion(zona);
            Ui.Pausa();
        }

        private static void Dibujar(Nexus nexus, Zona zona)
        {
            Cadete cadete = nexus.Cadete;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("NEXUS // " + zona.Nombre + " // " + zona.Titulo + "      NIVEL " + nexus.Nivel);
            Console.ForegroundColor = zona.ColorBorde;
            Console.WriteLine("╔█" + new string('█', Zona.Ancho * 2) + "█╗");
            for (int fila = 0; fila < Zona.Alto; fila++)
            {
                Console.ForegroundColor = zona.ColorBorde;
                Console.Write("██");
                for (int columna = 0; columna < Zona.Ancho; columna++)
                {
                    if (fila == zona.Fila && columna == zona.Columna)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("@");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(" ");
                    }
                    else
                    {
                        char terreno = zona.Mapa[fila, columna];
                        Console.ForegroundColor = zona.ColorTerreno(terreno);
                        Console.Write(terreno + " ");
                    }
                }
                Console.ForegroundColor = zona.ColorBorde;
                Console.WriteLine("██");
            }
            Console.ForegroundColor = zona.ColorBorde;
            Console.WriteLine("╚█" + new string('█', Zona.Ancho * 2) + "█╝");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Energía ");
            Console.ForegroundColor = Ui.ColorVida(cadete.Energia, cadete.EnergiaMaxima);
            Console.Write("[" + Ui.Barra(cadete.Energia, cadete.EnergiaMaxima, 10) + "] " + cadete.Energia + "/" + cadete.EnergiaMaxima);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   Estabilidad ");
            Console.ForegroundColor = Ui.ColorVida(cadete.Estabilidad, 100);
            Console.Write("[" + Ui.Barra(cadete.Estabilidad, 100, 10) + "] " + cadete.Estabilidad + "/100");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("   Escudo: " + cadete.Escudo + "   XP: " + cadete.Xp);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Pos [X: " + zona.Columna + ", Y: " + zona.Fila + "]   Terreno: " + NombreTerreno(zona.Mapa[zona.Fila, zona.Columna]) +
                              "   Anomalías: " + nexus.Anomalias + "   Inventario: " + nexus.Inventario.Cantidad + "/" + Inventario.Capacidad);
            Objetivo pendiente = nexus.MisionPrincipal.Pendiente;
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (pendiente != null) Console.WriteLine("▶ Objetivo: " + pendiente.Descripcion);
            else Console.WriteLine("🟢 Misión completada. Explora libremente.");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("[WASD] Mover [I] Inventario [U] Usar [F] Atacar [Z] Analizar [M] Misión [L] Leyenda [Q] Salir");
            Console.ForegroundColor = colorAviso;
            Console.WriteLine(ultimoAviso);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public static class Arte
    {
        public static string Inventario = "                                                        \r\n                                                        \r\n██ ▄▄  ▄▄ ▄▄ ▄▄ ▄▄▄▄▄ ▄▄  ▄▄ ▄▄▄▄▄▄ ▄▄▄  ▄▄▄▄  ▄▄  ▄▄▄  \r\n██ ███▄██ ██▄██ ██▄▄  ███▄██   ██  ██▀██ ██▄█▄ ██ ██▀██ \r\n██ ██ ▀██  ▀█▀  ██▄▄▄ ██ ▀██   ██  ██▀██ ██ ██ ██ ▀███▀ \r\n                                                        ";
        public static string Exploracion = "                                                                                                      \r\n                                                                                                      \r\n██████ ▄▄ ▄▄ ▄▄▄▄  ▄▄     ▄▄▄  ▄▄▄▄   ▄▄▄   ▄▄▄▄ ▄▄  ▄▄▄  ▄▄  ▄▄   ▄████▄  ▄▄▄▄ ▄▄▄▄▄▄ ▄▄ ▄▄ ▄▄  ▄▄▄  \r\n██▄▄   ▀█▄█▀ ██▄█▀ ██    ██▀██ ██▄█▄ ██▀██ ██▀▀▀ ██ ██▀██ ███▄██   ██▄▄██ ██▀▀▀   ██   ██ ██▄██ ██▀██ \r\n██▄▄▄▄ ██ ██ ██    ██▄▄▄ ▀███▀ ██ ██ ██▀██ ▀████ ██ ▀███▀ ██ ▀██   ██  ██ ▀████   ██   ██  ▀█▀  ██▀██ \r\n                                                                                                      ";
        public static string Identificacion = "┳┓┳┏┓┳┓┓┏┏┓┳┓┳┳┓┏┓  ┏┓┓   ┏┓┳┳┏┳┓┳┳┳┓┏┓╻\r\n┣┫┃┣ ┃┃┃┃┣ ┃┃┃┃┃┃┃  ┣┫┃   ┣ ┃┃ ┃ ┃┃┣┫┃┃┃\r\n┻┛┻┗┛┛┗┗┛┗┛┛┗┻┻┛┗┛  ┛┗┗┛  ┻ ┗┛ ┻ ┗┛┛┗┗┛•\r\n                                        ";

        public static void Presentacion()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("===========================================================================================================");
            Console.WriteLine("\u001b[0;37;40m┌\u001b[0;94;40m──\u001b[0;34;40m─┐\u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;37;40m ┐\u001b[0;94;40m  ┌\u001b[0;37;40m │\u001b[0;94;40m \u001b[0;95;40m·\u001b[0;94;40m \u001b[0;34;40m│\u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;34;40m─┐\u001b[0;37;40m      ├\u001b[0;94;40m─  \u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┐\u001b[0;37;40m ┌\u001b[0;94;40m──\u001b[0;34;40m─┐\u001b[0;37;40m └ ┌\u001b[0;94;40m──\u001b[0;34;40m─┐\u001b[0;37;40m └ ┌\u001b[0;94;40m──\u001b[0;34;40m─┐\u001b[0;37;40m ┌\u001b[0;94;40m──\u001b[0;34;40m─\u001b[0;94;40m \u001b[0;37;40m      ┌\u001b[0;94;40m─\u001b[0;34;40m─┐\u001b[0;37;40m └\u001b[0;94;40m─\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┤\u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;34;40m─┐\u001b[0;37;40m ├\u001b[0;94;40m─  \u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;37;40m ┌\u001b[0;94;40m─\u001b[0;37;40m┬\u001b[0;34;40m─┐\u001b[0m\r\n\u001b[0;94;40m│ \u001b[0;95;40m·\u001b[0;94;40m \u001b[0;34;40m│\u001b[0;37;40m \u001b[0;94;40m├\u001b[0;34;40m───\u001b[0;37;40m \u001b[0;94;40m└─\u001b[0;34;40m┬\u001b[0;37;40m┘ \u001b[0;94;40m└─\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┘\u001b[0;37;40m \u001b[0;94;40m└\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┐\u001b[0;37;40m      \u001b[0;94;40m└\u001b[0;34;40m──┘\u001b[0;37;40m \u001b[0;94;40m├\u001b[0;34;40m──\u001b[0;94;40m┐\u001b[0;34;40m┘\u001b[0;37;40m \u001b[0;94;40m├\u001b[0;34;40m───┤\u001b[0;37;40m \u001b[0;34;40m│\u001b[0;37;40m \u001b[0;94;40m│ \u001b[0;95;40m·\u001b[0;94;40m \u001b[0;34;40m│\u001b[0;37;40m \u001b[0;34;40m│\u001b[0;37;40m \u001b[0;94;40m│ \u001b[0;95;40m·\u001b[0;94;40m \u001b[0;34;40m│\u001b[0;37;40m \u001b[0;94;40m│\u001b[0;34;40m──\u001b[0;94;40m─┐\u001b[0;37;40m      \u001b[0;94;40m└\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┐\u001b[0;37;40m \u001b[0;34;40m└───┘\u001b[0;37;40m \u001b[0;94;40m└\u001b[0;34;40m─\u001b[0;94;40m─\u001b[0;34;40m┐\u001b[0;37;40m \u001b[0;94;40m└\u001b[0;34;40m──┘\u001b[0;37;40m \u001b[0;94;40m├\u001b[0;34;40m───\u001b[0;37;40m \u001b[0;94;40m│ \u001b[0;95;40m·\u001b[0;94;40m \u001b[0;34;40m│\u001b[0m\r\n\u001b[0;37;40m      \u001b[0;34;40m└───\u001b[0;37;40m \u001b[0;94;40m┘ \u001b[0;34;40m└\u001b[0;94;40m─\u001b[0;37;40m       \u001b[0;34;40m└──┘\u001b[0;37;40m                                       \u001b[0;94;40m   \u001b[0;33;40m─\u001b[0;34;40m┘\u001b[0;37;40m      \u001b[0;34;40m└──┘\u001b[0;37;40m       \u001b[0;34;40m└──┘\u001b[0;37;40m      \u001b[0;34;40m└───\u001b[0;37;40m      \u001b[0m");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("===========================================================================================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\r\n+++++-    -+++--++++++++++++++++++++.-+++++++++++++++++++++++++--++++\r\n+++-     -++++++-.++-+++++++++++++++++++--++++++++++-++++++++++++++++\r\n+++    ++++++++- .++-++++++++++++++++++++-+++++++++++++++--++++++++++\r\n+++   .++--++++-  -++++--++++++++++++++++++++++++++++++++++++++++++++");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("++++   ++++++++-   -+++++++++++++++++++++++++- -+++++++++++++++++++++\r\n++++++-.+++++++.   +++++-++++++++. ++++++++++. -++++++++++++++++-++++\r\n+++-+++++++++++-   -++    -++++++++++++++++++. .++++--+++++++++++++++\r\n+++++++++++++++++##+  .###+.-++++++++..+#+-++##+--+++++++- +--+++++++\r\n++++++++++++#++++#-  #######-       -######-       -+++---+#--+++++++\r\n++++-++++++##+--   .#+-   .-##+      .   .-         ++++-+###--+++--+\r\n+++++--..+#+##+  ####--   -.####-    -    -          +##+++#+++++-+++\r\n+++++-----#++  +#####--   --######-                  -+-.+###- .--+++");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("++++++++++-  -######################-                 +-.+##+++++++++\r\n++-+--. .  -##########################+#############-#####+#+##    -+\r\n++++++++++#+#+-#############################+.-.-+###+..-++####+--.-+\r\n++++-+####+## -#-.   --+##..    .-##-....+#---- .-####+-.++#--++##+++\r\n++-++++- .....-+--   --+##.-    --###---+##--    .###+.....-........-\r\n+++++++        #--   --+##.-    .-###---+##--    .###.\r\n               #+#####+###+##+++++###---+############.\r\n               ######################----############.\r\n               ###################-         #########\r\n                                      ....           ..          ..");
        }

        public static void Training()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("===========================================================================================================");
            Console.WriteLine("\u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀▀▄\u001b[0;37;40m  \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;94;40m█\u001b[0;37;40m  \u001b[0;94;40m▄▀▀▀\u001b[0;36;40m▀\u001b[0;94;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀█\u001b[0;37;40m \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;94;40m▀▀▀▀▀▄\u001b[0;37;40m  \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;94;40m█\u001b[0;37;40m \u001b[0;94;40m▓▀▀▀█\u001b[0;37;40m  \u001b[0;94;40m▄▀▀▀\u001b[0;36;40m▀\u001b[0;94;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀█\u001b[0;37;40m \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;94;40m▀▀▀▀▀▄\u001b[0;37;40m  \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;94;40m█\u001b[0;37;40m \u001b[0;97;40m█▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀▀▄\u001b[0;37;40m   \u001b[0;97;40m▄\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;96;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀\u001b[0;36;40m▀\u001b[0;94;40m▀▀▄\u001b[0;37;40m \u001b[0m\r\n\u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▄▄\u001b[0;37;40m \u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;40m█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▓\u001b[0;37;40m \u001b[0;94;40m█\u001b[0;97;40m·\u001b[0;37;40m   \u001b[0;94;40m▄▄▄▄▄\u001b[0;36;40m▄█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▄▄\u001b[0;37;40m    \u001b[0;94;40m█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▓\u001b[0;37;40m \u001b[0;94;40m▒\u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;40m█\u001b[0;37;40m \u001b[0;94;40m█\u001b[0;97;40m·\u001b[0;37;40m   \u001b[0;94;40m▄▄▄▄▄\u001b[0;36;40m▄█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▄▄\u001b[0;37;40m    \u001b[0;94;40m█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▓\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▄▄\u001b[0;37;40m \u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;40m█\u001b[0;37;40m \u001b[0;96;46m▀\u001b[0;37;40m    \u001b[0;94;40m▄▄\u001b[0;37;40m \u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;40m█\u001b[0m\r\n\u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▌\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m  \u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;94;40m▄▄▄▄▄▄\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▌\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▒\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m  \u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;94;40m▄▄▄▄▄▄\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▌\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▌\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m    \u001b[0;94;44m▓\u001b[0;37;40m \u001b[0;94;40m▌\u001b[0;37;40m   \u001b[0;94;44m▓\u001b[0m\r\n\u001b[0;94;44m▒\u001b[0;37;40m  \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;40m▀▀▀\u001b[0;37;40m \u001b[0;34;40m▀▄\u001b[0;37;40m  \u001b[0;94;44m▒\u001b[0;37;40m   \u001b[0;97;40m·\u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m ∙  \u001b[0;34;40m▄▄▄▄▄▄\u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m    \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m  \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m   \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m ∙  \u001b[0;34;40m▄▄▄▄▄▄\u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m    \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m   \u001b[0;97;40m·\u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m  \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;34;40m▄\u001b[0;94;44m▓\u001b[0;37;40m   \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m  \u001b[0;97;40m·\u001b[0;37;40m \u001b[0;94;44m▒\u001b[0;37;40m \u001b[0;94;44m▓\u001b[0;37;40m   \u001b[0;94;44m▒\u001b[0m\r\n\u001b[0;94;44m░\u001b[0;37;40m    \u001b[0;34;40m█▀▄\u001b[0;90;40m·\u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m    \u001b[0;94;44m░\u001b[0;34;40m▄▄▄▄▄▄\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m   \u001b[0;97;40m∙\u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m   \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m    \u001b[0;94;44m░\u001b[0;34;40m▄\u001b[0;94;44m░\u001b[0;90;40m·\u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m    \u001b[0;94;44m░\u001b[0;34;40m▄▄▄▄▄▄\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m   \u001b[0;97;40m∙\u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m   \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;97;40m.\u001b[0;37;40m  \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m       \u001b[0;90;40m·\u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m \u001b[0;94;44m░\u001b[0;37;40m    \u001b[0;94;44m░\u001b[0;34;40m▄\u001b[0;94;44m░\u001b[0;90;40m·\u001b[0;97;40m.\u001b[0;37;40m \u001b[0;94;44m░\u001b[0m\r\n\u001b[0;34;40m█\u001b[0;37;40m   .\u001b[0;34;40m▓▄▓\u001b[0;37;40m   \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m    \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m    \u001b[0;97;40m.\u001b[0;37;40m    \u001b[0;97;40m·\u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;97;40m∙\u001b[0;37;40m  \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m   \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m   .      \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m    \u001b[0;97;40m.\u001b[0;37;40m    \u001b[0;97;40m·\u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;97;40m∙\u001b[0;37;40m  \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m   \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m    \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m   .      \u001b[0;34;40m█\u001b[0;37;40m \u001b[0;34;40m█\u001b[0;37;40m   .      \u001b[0;34;40m█\u001b[0m\r\n\u001b[0;34;40m█▄▄▄▄▄▄▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄▄▄▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄█\u001b[0;37;40m  \u001b[0;34;40m▀▄▄▄▄▄▄▄▄▀\u001b[0;37;40m  \u001b[0;34;40m█▄▄▄▄▄▄▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄█\u001b[0;37;40m \u001b[0;34;40m█▄▄▄▄▄▄▄▄▄▄█\u001b[0;37;40m  \u001b[0;34;40m▀▄▄▄▄▄▄▄▄▀\u001b[0;37;40m \u001b[0m");
            Console.WriteLine("  NEXUS TRAINING SYSTEM");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("===========================================================================================================");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            // Orden importante: primero VT y fuente, después el tamaño de ventana
            ActivarVT();
            FijarFuente(12);
            ConfigurarConsola();

            PantallaCarga();

            Arte.Presentacion();

            string nombre = PedirNombre();
            int edad = PedirEdad();
            string realidad = PedirRealidad();

            Console.ForegroundColor = ConsoleColor.DarkRed;

            double energia = PedirNumero(
                "\nIngrese la energía: ",
                "Error: la energía debe tener valores min 0 y max 100"
            );

            double estabilidad = PedirNumero(
                "\nIngrese la estabilidad de conexión: ",
                "Error: la conexión debe estar entre 0 y 100."
            );

            Console.ForegroundColor = ConsoleColor.White;

            Cadete cadete = new Cadete(nombre, edad, realidad, energia, estabilidad);

            ValidarCondiciones(cadete);
            PantallaInicializacion();
            Arte.Training();
            MostrarResumen(cadete);

            Nexus nexus = new Nexus(cadete);

            MostrarBriefingMision(nexus);
            MenuPrincipal(nexus);
        }

        // PREGUNTA 8
        static void PruebaEstadoConsultable()
        {
            Cadete cadete = new Cadete("Rodrigo", 18, "IRIS-01", 80, 70);
            Robot robot = new Robot("DRON VIGÍA", 40, 5);

            IEstadoConsultable estadoCadete = cadete;
            IEstadoConsultable estadoRobot = robot;

            Console.WriteLine(estadoCadete.ObtenerResumenEstado());
            Console.WriteLine(estadoRobot.ObtenerResumenEstado());
            Console.ReadLine();
        }

        // Pregunta 9
        static void PruebaConsultaLinq()
        {
            List<Objeto> objetos = new List<Objeto>
            {
                Objeto.Crear(TipoObjeto.Escudo),
                Objeto.Crear(TipoObjeto.CelulaEnergia),
                Objeto.Crear(TipoObjeto.Escudo)
            };

            List<string> nombres = Inventario.ObtenerNombresPorTipo(objetos, TipoObjeto.Escudo);
            Console.WriteLine("Objetos encontrados:");
            foreach (string nombre in nombres)
            {
                Console.WriteLine(nombre);
            }

            List<string> sinCoincidencias = Inventario.ObtenerNombresPorTipo(objetos, TipoObjeto.NucleoResonancia);
            Console.WriteLine("Cantidad sin coincidencias: " + sinCoincidencias.Count);
        }

        // Pregunta 10
        static void PruebaAgregarEnergia()
        {
            Cadete cadete1 = new Cadete("Prueba 1", 18, "IRIS-01", 40, 70);
            cadete1.AgregarEnergia(20);
            Console.WriteLine("Cantidad 20: " + cadete1.Energia + " (esperado: 60)");

            Cadete cadete2 = new Cadete("Prueba 2", 18, "IRIS-01", 40, 70);
            cadete2.AgregarEnergia(90);
            Console.WriteLine("Cantidad 90: " + cadete2.Energia + " (esperado: 100)");

            Cadete cadete3 = new Cadete("Prueba 3", 18, "IRIS-01", 40, 70);
            cadete3.AgregarEnergia(-15);
            Console.WriteLine("Cantidad -15: " + cadete3.Energia + " (esperado: 40)");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            ref CONSOLE_FONT_INFOEX lpConsoleCurrentFont
        );

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int modo);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int modo);

        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct CONSOLE_FONT_INFOEX
        {
            public int cbSize;
            public int nFont;
            public short FontWidth;    
            public short FontHeight;   
            public int FontFamily;
            public int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        static short tamanoFuente = 12;

        static void FijarFuente(short altura)
        {
            try
            {
                CONSOLE_FONT_INFOEX fuente = new CONSOLE_FONT_INFOEX();
                fuente.cbSize = Marshal.SizeOf(typeof(CONSOLE_FONT_INFOEX));
                fuente.nFont = 0;
                fuente.FontWidth = 0;
                fuente.FontHeight = altura;
                fuente.FontFamily = 54;
                fuente.FontWeight = 400;
                fuente.FaceName = "Consolas";

                SetCurrentConsoleFontEx(GetStdHandle(-11), false, ref fuente);
                tamanoFuente = altura;
            }
            catch
            {
                
            }
        }

        static void ActivarVT()
        {
            try
            {
                IntPtr handle = GetStdHandle(-11);
                int modo;
                if (GetConsoleMode(handle, out modo))
                    SetConsoleMode(handle, modo | 0x0004); 
            }
            catch
            {
                
            }
        }

        

        static void ConfigurarConsola()
        {
            try
            {
                Console.Title = "NEXUS Training System";

                int anchoMaximo = Console.LargestWindowWidth;
                int altoMaximo = Console.LargestWindowHeight;

                int ancho = Math.Min(170, anchoMaximo);
                int alto = Math.Min(50, altoMaximo);

                if (ancho < 100) ancho = Math.Min(100, anchoMaximo);
                if (alto < 35) alto = Math.Min(35, altoMaximo);

                
                Console.SetBufferSize(ancho, alto);
                Console.SetWindowSize(ancho, alto);

                Console.CursorVisible = false;
            }
            catch
            {
                
            }
        }

        

        static readonly Regex AnsiRegex = new Regex(@"\u001B\[[0-9;?]*[A-Za-z]", RegexOptions.Compiled);
        static readonly Regex ColorRegex = new Regex(@"\u001B\[[0-9;]*m", RegexOptions.Compiled);

        static void ReproducirAnimacion()
        {
            string carpeta = Path.Combine(AppContext.BaseDirectory, "frames");

            if (!Directory.Exists(carpeta))
            {
                Console.WriteLine();
                Console.WriteLine("No se encontró la carpeta frames.");
                Thread.Sleep(1500);
                return;
            }

            const int FPS = 20;
            const int DURACION_MAXIMA = 5;

            
            int ancho, alto;
            List<string[]> crudos = LeerFrames(carpeta, FPS * DURACION_MAXIMA, out ancho, out alto);
            if (crudos.Count == 0) return;

            
            if (!AjustarConsola(ancho, alto))
            {
                Console.WriteLine();
                Console.WriteLine("La ventana es muy pequeña para la animación (" + ancho + "x" + alto + "). Se omite.");
                Thread.Sleep(1500);
                return;
            }

            
            byte[][] datos = PrepararFrames(crudos, ancho);

            
            Stream salida = Console.OpenStandardOutput();

            SoundPlayer sonido = null;
            string rutaSonido = Path.Combine(AppContext.BaseDirectory, "Sonidos", "DespegueCohete.wav");
            if (File.Exists(rutaSonido))
            {
                sonido = new SoundPlayer(rutaSonido);
                sonido.Play();
            }

            Console.CursorVisible = false;
            Enviar(salida, Encoding.UTF8.GetBytes("\u001b[?1049h\u001b[2J\u001b[H"));

            int msPorFrame = 1000 / FPS;
            Stopwatch reloj = Stopwatch.StartNew();
            int ultimo = -1;

            while (true)
            {
                
                int actual = (int)(reloj.ElapsedMilliseconds / msPorFrame);
                if (actual >= datos.Length) break;

                if (actual != ultimo)
                {
                    Enviar(salida, datos[actual]);
                    ultimo = actual;
                }
                else Thread.Sleep(1);
            }

            reloj.Stop();
            if (sonido != null) sonido.Stop();

            Enviar(salida, Encoding.UTF8.GetBytes("\u001b[0m\u001b[?1049l"));
            Console.CursorVisible = true;

            
            if (tamanoFuente != 12)
            {
                FijarFuente(12);
                ConfigurarConsola();
            }

            Console.Clear();
        }

        static void Enviar(Stream salida, byte[] bytes)
        {
            salida.Write(bytes, 0, bytes.Length);   
            salida.Flush();
        }

        static List<string[]> LeerFrames(string carpeta, int maximo, out int ancho, out int alto)
        {
            ancho = 0;
            alto = 0;
            List<string[]> frames = new List<string[]>();

            string[] archivos = Directory.GetFiles(carpeta, "*.txt")
                .OrderBy(x => x)
                .Take(maximo)
                .ToArray();

            foreach (string archivo in archivos)
            {
                
                string[] lineas = File.ReadAllText(archivo).Replace("\r", "").TrimEnd('\n').Split('\n');
                frames.Add(lineas);

                if (lineas.Length > alto) alto = lineas.Length;

                foreach (string linea in lineas)
                {
                    int visible = AnsiRegex.Replace(linea, "").Length;
                    if (visible > ancho) ancho = visible;
                }
            }
            return frames;
        }

        static byte[][] PrepararFrames(List<string[]> frames, int anchoArte)
        {
            
            int margen = Math.Max(0, (Console.WindowWidth - anchoArte) / 2);
            string sangria = new string(' ', margen);
            UTF8Encoding utf8 = new UTF8Encoding(false);
            byte[][] resultado = new byte[frames.Count][];

            for (int f = 0; f < frames.Count; f++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\u001b[?2026h");                        

                string[] lineas = frames[f];
                for (int i = 0; i < lineas.Length; i++)
                {
                    sb.Append("\u001b[").Append(i + 1).Append(";1H"); 
                    sb.Append("\u001b[0m").Append(sangria);
                    sb.Append(CompactarColores(lineas[i]));
                    sb.Append("\u001b[0m\u001b[K");                    
                }

                sb.Append("\u001b[?2026l");                          
                resultado[f] = utf8.GetBytes(sb.ToString());
            }
            return resultado;
        }

        
        static string CompactarColores(string linea)
        {
            StringBuilder sb = new StringBuilder();
            string ultimo = null;
            int pos = 0;

            foreach (Match m in ColorRegex.Matches(linea))
            {
                sb.Append(linea, pos, m.Index - pos);
                if (m.Value != ultimo)
                {
                    sb.Append(m.Value);
                    ultimo = m.Value;
                }
                pos = m.Index + m.Length;
            }
            sb.Append(linea, pos, linea.Length - pos);
            return sb.ToString();
        }

        static bool AjustarConsola(int ancho, int alto)
        {
            if (Console.WindowWidth >= ancho && Console.WindowHeight >= alto) return true;

            short[] tamanos = { 12, 10, 8, 6 };
            foreach (short t in tamanos)
            {
                FijarFuente(t);
                if (Console.LargestWindowWidth >= ancho && Console.LargestWindowHeight >= alto)
                {
                    try
                    {
                        int w = Math.Max(Console.WindowWidth, ancho);
                        int h = Math.Max(Console.WindowHeight, alto);
                        Console.SetBufferSize(Math.Max(Console.BufferWidth, w), Math.Max(Console.BufferHeight, h));
                        Console.SetWindowSize(w, h);
                        Console.SetBufferSize(w, h);
                    }
                    catch
                    {
                        
                    }
                    break;
                }
            }
            return Console.WindowWidth >= ancho && Console.WindowHeight >= alto;
        }

     

        static void PantallaCarga()
        {
            ReproducirAnimacion();

            

            SoundPlayer sonidoInicio = null;
            string rutaSonidoInicio = Path.Combine(AppContext.BaseDirectory, "Sonidos", "Inicio2.wav");
            if (File.Exists(rutaSonidoInicio))
            {
                sonidoInicio = new SoundPlayer(rutaSonidoInicio);
                sonidoInicio.Play();
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine();
            Console.WriteLine("     INICIANDO CONEXIÓN NEXUS");
            Console.WriteLine();

            for (int i = 0; i <= 10; i++)
            {
                Console.SetCursorPosition(5, Console.CursorTop);
                Console.Write("[" + new string('█', i) + new string(' ', 10 - i) + "]");
                Thread.Sleep(150);
            }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("     ✓ CONEXIÓN ESTABLECIDA");
            Console.ResetColor();

            if (sonidoInicio != null) sonidoInicio.Stop();
        }

        static bool TieneNumeros(string texto)
        {
            foreach (char caracter in texto)
            {
                if (char.IsDigit(caracter)) return true;
            }
            return false;
        }

        static SoundPlayer musicaJuego;

        static void IniciarMusicaJuego()
        {
            if (musicaJuego != null) return;

            string ruta = Path.Combine(AppContext.BaseDirectory, "Sonidos", "MusicaJuego.wav");
            if (File.Exists(ruta))
            {
                musicaJuego = new SoundPlayer(ruta);
                musicaJuego.PlayLooping();
            }
        }

        static string PedirNombre()
        {
            IniciarMusicaJuego();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Arte.Identificacion);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("              ───── IDENTIFICACIÓN ─────");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\nIngrese su nombre: ");

                Console.ForegroundColor = ConsoleColor.White;
                string nombre = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    Console.WriteLine("Error: El nombre no puede estar vacío.");
                }
                else if (TieneNumeros(nombre))
                {
                    Console.WriteLine("Error: El nombre no puede contener números.");
                }
                else
                {
                    return nombre;
                }
            }
        }

        static int PedirEdad()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\nIngrese su edad: ");
                int edad;
                Console.ForegroundColor = ConsoleColor.White;
                if (int.TryParse(Console.ReadLine(), out edad) == false) Console.WriteLine("Error: debe ingresar un número.");
                else if (edad < 10 || edad > 100) Console.WriteLine("Error: la edad debe estar entre 10 y 100.");
                else return edad;
            }
        }

        static string PedirRealidad()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\nIngrese la realidad asignada: ");
                Console.ForegroundColor = ConsoleColor.White;
                string realidad = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(realidad)) Console.WriteLine("Error: La realidad asignada no puede estar vacio.");
                else if (TieneNumeros(realidad)) Console.WriteLine("Error: La realidad no puede contener números, solo letras.");
                else return realidad;
            }
        }

        static double PedirNumero(string pregunta, string mensajeRango)
        {
            while (true)
            {
                Console.Write(pregunta);
                double numero;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                if (double.TryParse(Console.ReadLine(), out numero) == false) Console.WriteLine("Error: debe ingresar un número válido.");
                else if (numero < 0 || numero > 100) Console.WriteLine(mensajeRango);
                else return numero;
            }
        }

        static void MensajeCondicion(string mensaje)
        {
            Console.WriteLine(mensaje);
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
            Console.Clear();
        }

        static void ValidarCondiciones(Cadete cadete)
        {
            int edad = cadete.Edad;
            if (edad > 18 && cadete.Energia > 90 && cadete.Estabilidad > 90) MensajeCondicion("\nEstas en las mejores condiciones para empezar!!!");
            if (edad < 18 && cadete.Energia < 25) MensajeCondicion("\nNo son las condiciones más aconsejables, necesitaras de más experiencia en la vida \n pero puedes continuar...");
            if (edad > 70 && cadete.Energia < 50 && cadete.Estabilidad > 80) MensajeCondicion("\nTu edad, no es la mejor...Necesitas subir tu energía más adelante. ");
            if (edad < 30 && cadete.Energia > 80 && cadete.Estabilidad > 60) MensajeCondicion("\nPerfecto!!!Puedes ingresar");
            if (edad > 10 && cadete.Energia > 50 && cadete.Estabilidad > 70) MensajeCondicion("\nTu edad no es la mejor, pero tus demás atributos son factibles, puedes continuar!");
            if (cadete.Energia < 15)
            {
                Console.WriteLine("\nTu energia es muy pobre, no podras continuar, ingresa nuevamente...");
                Environment.Exit(0);
            }
            if (cadete.Estabilidad < 15)
            {
                Console.WriteLine("\nTu conexión es demasidos debil, ingresa nuevamente...");
                Environment.Exit(0);
            }
        }

        static void PantallaInicializacion()
        {
            Console.WriteLine("────────────────────────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine("                INICIALIZANDO NEXUS...");
            Console.WriteLine();
            Console.Write("        ███████████████████░░░░░░░░░  65%");
            Thread.Sleep(500);
            Console.Write("\r        ████████████████████████░░░░░  80%");
            Thread.Sleep(500);
            Console.Write("\r        █████████████████████████████  100%");
            Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("────────────────────────────────────────────────────────────");
            Thread.Sleep(1000);
            Console.Clear();
        }

        static void MostrarResumen(Cadete cadete)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n");
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║                 NEXUS SYSTEM                 ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║  EXPLORADOR:  " + cadete.Nombre.PadRight(30) + "║");
            Console.WriteLine("║  REALIDAD:    " + cadete.Realidad.PadRight(30) + "║");
            Console.WriteLine("║  ENERGÍA:     " + cadete.Energia.ToString().PadRight(30) + "║");
            Console.WriteLine("║  ESTABILIDAD: " + cadete.Estabilidad.ToString().PadRight(30) + "║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
        }

        static void MostrarBriefingMision(Nexus nexus)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Año 3500. El Cadete " + nexus.Cadete.Nombre + " recibe una nueva misión:");
            Console.WriteLine("investigar una anomalía detectada en una realidad desconocida.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📋 MISIÓN: " + nexus.MisionPrincipal.Titulo);
            Console.ForegroundColor = ConsoleColor.White;
            foreach (Objetivo objetivo in nexus.MisionPrincipal.Objetivos) Console.WriteLine("   [ ] " + objetivo.Descripcion);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Kit inicial: 🔋 Célula de energía, 🛡️ Escudo, 🔬 Analizador.");
            Console.WriteLine("Explora una realidad (opción 1), administra tu inventario y regresa con vida.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nPresione ENTER para comenzar...");
            Console.ReadLine();
            Console.Clear();
        }

        static void MenuPrincipal(Nexus nexus)
        {
            Cadete cadete = nexus.Cadete;
            int opcion = 0;
            while (opcion != 8)
            {
                MostrarMenu(nexus);
                if (int.TryParse(Console.ReadLine(), out opcion) == false)
                {
                    Ui.Escribir("Error: debes ingresar una opción del 1 al 8.", ConsoleColor.Red);
                    Console.ForegroundColor = ConsoleColor.White;
                    continue;
                }
                if (opcion < 1 || opcion > 8)
                {
                    Ui.Escribir("Opción no válida.", ConsoleColor.Red);
                    Console.ForegroundColor = ConsoleColor.White;
                    continue;
                }
                bool pausar = true;
                if (opcion == 1)
                {
                    Exploracion.MenuRealidades(nexus);
                    Console.Clear();
                    pausar = false;
                }
                else if (opcion == 2)
                {
                    if (AnalizarAnomalia(nexus) == false) continue;
                }
                else if (opcion == 3) DiagnosticoEnlace(cadete);
                else if (opcion == 4)
                {
                    if (RecargarEnergia(cadete) == false) continue;
                }
                else if (opcion == 5)
                {
                    if (EstabilizarEnlace(cadete) == false) continue;
                }
                else if (opcion == 6)
                {
                    nexus.MostrarInventario();
                    Console.Clear();
                    pausar = false;
                }
                else if (opcion == 7)
                {
                    nexus.MostrarMisiones();
                    Console.Clear();
                    pausar = false;
                }
                else Desconectar();
                bool huboMensaje = RevisarEstados(nexus);
                if (pausar || huboMensaje)
                {
                    Console.WriteLine("\nPresione ENTER para continuar...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        static void MostrarMenu(Nexus nexus)
        {
            Cadete cadete = nexus.Cadete;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==============================================================");
            Console.WriteLine("                    NEXUS // NIVEL " + nexus.Nivel);
            Console.WriteLine("==============================================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Energía: " + cadete.Energia + "/100      Estabilidad: " + cadete.Estabilidad + "/100      Anomalías: " + nexus.Anomalias + "      XP: " + cadete.Xp);
            Console.WriteLine();
            Console.WriteLine("1. Explorar realidad");
            Console.WriteLine("2. Analizar anomalía");
            Console.WriteLine("3. Diagnóstico de enlace");
            Console.WriteLine("4. Recuperar energía");
            Console.WriteLine("5. Estabilizar enlace");
            Console.WriteLine("6. Inventario");
            Console.WriteLine("7. Misiones");
            Console.WriteLine("8. Desconectarse");
            Console.Write("\nIngrese una opción: ");
        }

        static bool AnalizarAnomalia(Nexus nexus)
        {
            Cadete cadete = nexus.Cadete;
            Console.WriteLine("\nANALIZAR ANOMALÍA");
            if (nexus.Anomalias <= 0)
            {
                Ui.Escribir("No tienes anomalías para analizar.", ConsoleColor.Yellow);
                Console.WriteLine("Primero debes explorar la realidad.");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            if (cadete.Energia < 10)
            {
                Ui.Escribir("No tienes suficiente energía.", ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            double estabilidadAntes = cadete.Estabilidad;
            cadete.AnalizarAnomalia();
            nexus.EliminarAnomalia();
            if (estabilidadAntes >= 60)
            {
                Ui.Escribir("ANÁLISIS ESTABLE.", ConsoleColor.Green);
                Console.WriteLine("La anomalía fue comprendida correctamente.");
                Console.WriteLine("+5 estabilidad.");
            }
            else
            {
                Ui.Escribir("ANÁLISIS INESTABLE.", ConsoleColor.Red);
                Console.WriteLine("La anomalía provocó interferencias.");
                Console.WriteLine("-5 estabilidad.");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Energía actual: " + cadete.Energia);
            Console.WriteLine("Estabilidad actual: " + cadete.Estabilidad);
            nexus.GanarXp(10);
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }

        static void DiagnosticoEnlace(Cadete cadete)
        {
            Console.WriteLine("\nDIAGNÓSTICO DE ENLACE");
            if (cadete.Estabilidad >= 80)
            {
                Ui.Escribir("ENLACE MUY ESTABLE.", ConsoleColor.Green);
                Console.WriteLine("La realidad responde correctamente.");
            }
            else if (cadete.Estabilidad >= 50)
            {
                Ui.Escribir("ENLACE INESTABLE.", ConsoleColor.Yellow);
                Console.WriteLine("Conviene estabilizar antes de seguir explorando.");
            }
            else
            {
                Ui.Escribir("ENLACE CRÍTICO.", ConsoleColor.Red);
                Console.WriteLine("Una nueva exploración podría ser peligrosa.");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        static bool RecargarEnergia(Cadete cadete)
        {
            Console.WriteLine("\nRECARGAR NÚCLEO ENERGÉTICO");
            if (cadete.Estabilidad <= 20)
            {
                Ui.Escribir("La estabilidad es demasiado baja para realizar una recarga.", ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            if (cadete.Energia >= 100)
            {
                Ui.Escribir("La energía ya está al máximo.", ConsoleColor.Yellow);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            cadete.RecuperarEnergia();
            Ui.Escribir("+25 energía", ConsoleColor.Green);
            Ui.Escribir("-5 estabilidad por interferencia", ConsoleColor.Yellow);
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }

        static bool EstabilizarEnlace(Cadete cadete)
        {
            Console.WriteLine("\nESTABILIZAR ENLACE");
            if (cadete.Energia < 15)
            {
                Ui.Escribir("Necesitas al menos 15 de energía para estabilizar el enlace.", ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            cadete.EstabilizarEnlace();
            Ui.Escribir("-15 energía", ConsoleColor.Green);
            Console.WriteLine("+15 estabilidad");
            Console.WriteLine("El enlace ha sido reforzado.");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }

        static void Desconectar()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\nFinalizando conexión...");
            for (int i = 0; i <= 100; i += 10)
            {
                Console.Write("\rDesconectando: [" + new string('█', i / 10) + new string('░', 10 - i / 10) + "] " + i + "%");
                Thread.Sleep(150);
            }
            Console.WriteLine("\n\nEnlace NEXUS cerrado correctamente.");
            Thread.Sleep(700);
            Environment.Exit(0);
        }

        static bool RevisarEstados(Nexus nexus)
        {
            Cadete cadete = nexus.Cadete;
            bool mostro = false;
            if (cadete.Energia >= 80 && cadete.Estabilidad >= 80 && nexus.ResonanciaActivada == false)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n*** RESONANCIA NEXUS DETECTADA ***");
                Console.WriteLine("Has alcanzado una sincronización excepcional.");
                cadete.AumentarEstabilidad(10);
                Console.WriteLine("+10 estabilidad");
                Console.WriteLine("NUEVO NIVEL DESBLOQUEADO.");
                nexus.SubirNivel();
                nexus.ActivarResonancia();
                Console.ForegroundColor = ConsoleColor.White;
                mostro = true;
            }
            if (cadete.Estabilidad <= 30 && cadete.Energia <= 30)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\n!!! ESTADO CRÍTICO !!!");
                Console.WriteLine("La realidad comienza a colapsar.");
                Console.WriteLine("Debes recuperar energía o estabilizar el enlace.");
                Console.ForegroundColor = ConsoleColor.White;
                mostro = true;
            }
            if (nexus.Anomalias >= 3 && nexus.Nivel == 1)
            {
                nexus.SubirNivel();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n*** NIVEL 2 DESBLOQUEADO ***");
                Console.WriteLine("Las anomalías están revelando nuevos patrones.");
                Console.ForegroundColor = ConsoleColor.White;
                mostro = true;
            }
            return mostro;
        }
    }
}
