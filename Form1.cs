
using System.Text.Json.Nodes;
namespace gato_be_check;

public partial class Form1 : Form
{
    Label info = new();
    Label moreInfo = new();

    FlowLayoutPanel fp = new();

    public Form1()
    {
        InitializeComponent();
        Button btn = new Button
        {
            Location = new Point(10, 20),
            AutoSize = true,
            Text = "Press this to connect to the League Client"
        };
        btn.Click += new EventHandler(Button_Click);

        info.Text = "Not connected yet.";
        info.AutoSize = true;
        info.Font = new Font(info.Font.Name, 24, info.Font.Style);

        moreInfo.Text = "";
        moreInfo.AutoSize = true;
        moreInfo.Font = new Font(moreInfo.Font.Name, 14, moreInfo.Font.Style);

        fp.FlowDirection = FlowDirection.TopDown;
        fp.Size = new(600,400);
        fp.AutoSize = true;
        fp.AutoSizeMode = AutoSizeMode.GrowOnly;
        fp.Controls.Add(btn);
        fp.Controls.Add(info);
        fp.Controls.Add(moreInfo);

        Controls.Add(fp);
        Update();
    }

    private (string exe, ushort pid, ushort port, string token, string proto)? readLockfile()
    {
        try
        {
            // Open the text file using a stream reader.
            using StreamReader reader = new(new FileStream("C:\\Riot Games\\League of Legends\\lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // Read the stream as a string.
            string text = reader.ReadToEnd();

            // Write the text to the console.
            Console.WriteLine(text);
            string[] data = text.Split(':');
            ushort pid = ushort.Parse(data[1]);
            ushort port = ushort.Parse(data[2]);

            return (data[0], pid, port, data[3], data[4]);
        }
        catch (IOException e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
            return null;
        }
        catch (FormatException e)
        {
            Console.WriteLine("Number format exception:");
            Console.WriteLine(e.Message);
            return null;
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine("Lockfile format exception:");
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private async void Button_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("button pressed");
        var lockfile = readLockfile();
        if (!lockfile.HasValue)
        {
            Console.WriteLine("ooooof");
            info.Text = "Please open the League Client and try again.";

            return;
        }
        var (exe, pid, port, token, proto) = lockfile.Value;

        Console.WriteLine($"{exe} is listening at {port}");

        // await HttpHelper.GetAsync()
        LCUClient lcu = new(port, token, proto);

        JsonNode? elm = await lcu.Get("/lol-loot/v1/player-loot");

        var champsBlueEssence = elm != null ? (
            elm.
            AsArray()
            .Where(e => e["displayCategories"].GetValue<string>() == "CHAMPION")
            .Sum(e => e["disenchantValue"].GetValue<long>() * e["count"].GetValue<long>())
        ) : 0;
        Console.WriteLine(champsBlueEssence);

        elm = await lcu.Get("/lol-inventory/v1/wallet/me");
        Console.WriteLine(elm);
        var accountBlueEssence = elm["lol_blue_essence"].GetValue<long>();
        var orangeEssence = elm["lol_orange_essence"].GetValue<long>();
        var mythicEssence = elm["lol_mythic_essence"].GetValue<long>();


        lcu.Dispose();

        var totalBlueEssence = champsBlueEssence + accountBlueEssence;

        info.Text = $"Wow! You have a total of {totalBlueEssence} BE!\n  - Champion shards: {champsBlueEssence}\n  - Account: {accountBlueEssence}";
        moreInfo.Text = $"\nAlso, you have {orangeEssence} Orange Essence and {mythicEssence} Mythic Essence. Nice.";

    }
}
