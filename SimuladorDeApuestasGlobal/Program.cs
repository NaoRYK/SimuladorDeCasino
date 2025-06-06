using Apostador.Models;
using System.Text;



//Output
string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);


int numSimulations = 10;
int ruinedSessionsCount = 0;


// Saldo por tirada
List<double> balanceHistory = new List<double>();


int vecesApostadas = 0;

//  frecuencia de premios
//   diccionario que cuente cuántas veces salió cada descripción
//   reward  Description como clave
Dictionary<string, int> frequency = new Dictionary<string, int>();

//  Rachas
int currentWinStreak = 0;
int currentLossStreak = 0;
int maxWinStreak = 0;
int maxLossStreak = 0;

//  Ruina / Objetivo ×2
bool ruined = false;
bool reachedTarget = false;
int tiradasHastaFin = 0;

double saldoInicial = 100000;
double saldo = saldoInicial;


int coste = 1000;

List<Reward> obtainedRewards = new List<Reward>();


List<Reward> rewardList = new List<Reward>()
{
    // Mega Jackpot: ×200 de la apuesta (200 000 pesos). Probabilidad = 0.0001 (0.01 %).
    new Reward(200_000, 0.0001,  "Mega Jackpot(x200)"),

    // Gran Jackpot: ×50 de la apuesta (50 000 pesos). Probabilidad = 0.0005 (0.05 %).
    new Reward(50_000,  0.0005,  "Gran Jackpot(x50)"),

    // Premio Medio: ×5 de la apuesta (5 000 pesos). Probabilidad = 0.03 (3 %).
    new Reward(5_000,   0.03,    "Premio Medio(x5)"),

    // Premio Pequeño: ×1 de la apuesta (1 000 pesos). Probabilidad = 0.60 (60 %).
    new Reward(1_000,   0.60,    "Premio Pequeño(x1)"),

    // Reembolso Parcial: ×0.5 de la apuesta (500 pesos). Probabilidad = 0.25 (25 %).
    new Reward(500,     0.25,    "Reembolso Parcial(x0.5)"),

    // Sin Premio: 0 pesos. Probabilidad = 0.1194 (11.94 %).
    new Reward(0,       0.1194,  "Sin Premio")
};

// Inicializo el diccionario de frecuencia en 0 para cada descripción
foreach (var r in rewardList)
{
    frequency[r.Description] = 0;
}
void Apostar()
{
    if (saldo < coste)
    {
        // No puedo apostar más → ruina
        ruined = true;
        tiradasHastaFin = vecesApostadas;
        return;
    }

    vecesApostadas++;

    // Restar coste y obtener premio
    saldo -= coste;
    Reward obtainedReward = getReward();

    // Actualizar frecuencia de premios
    frequency[obtainedReward.Description]++;

    //  Calcular win vs loss
    if (obtainedReward.Amount >= coste)
    {

        currentWinStreak++;
        maxWinStreak = Math.Max(maxWinStreak, currentWinStreak);


        currentLossStreak = 0;
    }
    else
    {

        currentLossStreak++;
        maxLossStreak = Math.Max(maxLossStreak, currentLossStreak);


        currentWinStreak = 0;
    }


    balanceHistory.Add(saldo);


    if (saldo < coste)
    {
        ruined = true;
        tiradasHastaFin = vecesApostadas;
    }
    else if (saldo >= 2 * saldoInicial)
    {
        reachedTarget = true;
        tiradasHastaFin = vecesApostadas;
    }

    // 3.7. Mostrar estado en consola (opcional)
    Console.Clear();
    Console.WriteLine($"Tirada #{vecesApostadas}: Obtuvo {obtainedReward.Description} (${obtainedReward.Amount}).");
    Console.WriteLine($"Saldo ahora: ${saldo}");
    Console.WriteLine($"Racha actual: Wins={currentWinStreak} Losses={currentLossStreak}");
    Console.WriteLine($"Max Rachas: MaxWins={maxWinStreak}  MaxLosses={maxLossStreak}");
    Console.WriteLine();
    Console.WriteLine("Frecuencia de premios hasta ahora:");
    foreach (var kvp in frequency)
    {
        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
    }
    Console.WriteLine();
    Console.WriteLine("Presiona X para detener manualmente.");
}


Reward getReward()
{
    double randomNumber = Random.Shared.NextDouble(); //genero el numero random
    double ac = 0;


    foreach (Reward reward in rewardList)
    { //mapeo los rewards
        ac += reward.Probability;//Hago las cumulative weights para sacar una recompensa random

        if (randomNumber < ac)
        {
            reward.countGain();
            return reward;
        }
    }
    Console.WriteLine(randomNumber);

    return rewardList.Last();

}
void ResetSession()
{
    // 1) Saldo
    saldo = saldoInicial;

    // 2) Tiradas
    vecesApostadas = 0;

    // 3) Ruina / objetivo
    ruined = false;
    reachedTarget = false;
    tiradasHastaFin = 0;

    // 4) Rachas
    currentWinStreak = 0;
    currentLossStreak = 0;
    maxWinStreak = 0;
    maxLossStreak = 0;

    // 5) Historial de saldo
    balanceHistory = new List<double>();

    // 6) Inicializar frecuencia en 0 para cada
    frequency = new Dictionary<string, int>();
    foreach (var r in rewardList)
    {
        frequency[r.Description] = 0;
        // reseteamos el contador interno gainedTimes
        r.resetGainedTimes();
    }
}



void ExportBalanceHistory(string path, List<double> history)
{
    var sb = new StringBuilder();
    sb.AppendLine("Tirada,Saldo");

    for (int i = 0; i < history.Count; i++)
    {
        sb.AppendLine($"{i + 1},{history[i]}");
    }

    File.WriteAllText(path, sb.ToString());
}

void ExportSummary(string path, int sessionIndex)
{
    var sb = new StringBuilder();
    sb.AppendLine("Sesión,SaldoInicial,SaldoFinal,TiradasHastaFin,Ruina,ReachedTargetX2,MaxWinStreak,MaxLossStreak,TotalTiradas");

    // Saldo final
    double finalBalance = ruined ? 0 : saldo;

    sb.AppendLine($"{sessionIndex},{saldoInicial},{finalBalance},{tiradasHastaFin},{ruined},{reachedTarget},{maxWinStreak},{maxLossStreak},{vecesApostadas}");

    // agregamos la frecuencia de cada premio
    string freqLine = sessionIndex.ToString();
    foreach (var r in rewardList)
    {
        // Reemplazamos comas en la descripción por "_" para no romper el CSV
        string key = r.Description.Replace(",", "_");
        freqLine += $",{frequency[key]}";
    }
    sb.AppendLine("");                // línea en blanco
    sb.AppendLine("Sesion," +         // cabeceras de frecuencia
                  string.Join(",", rewardList.ConvertAll(r => r.Description.Replace(",", "_"))));
    sb.AppendLine(freqLine);

    File.WriteAllText(path, sb.ToString());
}

//Stringbuilder global
var allSessionsSb = new StringBuilder();

allSessionsSb.Append("Sesion,SaldoInicial,SaldoFinal,TiradasHastaFin,Ruina,ReachedTargetX2,MaxWinStreak,MaxLossStreak,TotalTiradas");
foreach (var r in rewardList)
{
    string colName = r.Description.Replace(",", "_");
    allSessionsSb.Append($",{colName}");
}
allSessionsSb.AppendLine();



for (int s = 1; s <= numSimulations; s++)
{
    // Reiniciar todas las variables para la nueva sesión 
    ResetSession();

    // Bucle interno de tiradas
    while (!ruined && !reachedTarget)
    {
        Apostar();

    }

    //  Contar si esta sesión terminó en ruina
    if (ruined)
        ruinedSessionsCount++;

    //  Acumular el resumen de esta sesión en el StringBuilder global

    double finalBal = ruined ? 0 : saldo;
    var lineSb = new StringBuilder();
    lineSb.Append($"{s},{saldoInicial},{finalBal},{tiradasHastaFin},{ruined},{reachedTarget},{maxWinStreak},{maxLossStreak},{vecesApostadas}");

    foreach (var r in rewardList)
    {
        lineSb.Append($",{frequency[r.Description]}");
    }
    allSessionsSb.AppendLine(lineSb.ToString());


    //  guardamos el balance history de la simulación N°1”
    if (s == 1)
    {
        string pathBalance1 = Path.Combine(outputDir, $"balance_s{s}.csv");
        ExportBalanceHistory(pathBalance1, balanceHistory);
    }

    // exportar cada sesión en un CSV aparte  (cuidado que son mas de 10k  archivos eh)
    // ExportSummary($"summary_s{s}.csv", s);

}

//Escribimos todo en el csv global
string pathAllSummary = Path.Combine(outputDir, "all_sessions_summary.csv");
File.WriteAllText(pathAllSummary, allSessionsSb.ToString());

// mostrar probabilidad de ruina calculada
double probRuina = (double)ruinedSessionsCount / numSimulations;
Console.WriteLine($"Probabilidad de ruina tras {numSimulations} simulaciones: {probRuina:P2}");
Console.WriteLine("CSV de resumen guardado como: all_sessions_summary.csv");

//  guardar un CSV con el resumen de la primera sesión
string pathSummary1 = Path.Combine(outputDir, "summary_s1.csv");
ExportSummary(pathSummary1, 1);

// Pausa para salir
Console.WriteLine("Presiona cualquier tecla para salir.");
Console.ReadKey();