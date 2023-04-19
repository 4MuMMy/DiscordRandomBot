using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections;

using Newtonsoft.Json;
using DiscordRandomBot;

DiscordClient discord;

static async Task<string> GetResponseFromURI(string url)
{
    Uri u = new Uri(url);
    var response = "";


    int kacOldu = 0;
    bool resultStatusErr = true;
    bool timeOut = false;
    do
    {
        try
        {
            kacOldu++;
            using (var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 8) })
            {
                HttpResponseMessage result = await client.GetAsync(u);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
                resultStatusErr = !result.IsSuccessStatusCode;
            }
        }
        catch (TaskCanceledException)
        {
            timeOut = true;
        }
        catch
        {
        }

        //timeout olduysa direk çıkıyoruz
        if (timeOut) break;
        //* kez * ms aralıklarla bağlanmayı tekrar dene, * sn geçtikten sonra hâlâ bağlantı yoksa sonlandır
        //başarısız bağlantı için || başarısız token eşleşmesi için || bilinmeyen başarısız denemeler için
        else if (resultStatusErr || response == "yok" || response == "" || response == null)
        {
            if (kacOldu >= 20) break;
            else
            {
                await Task.Delay(100);
            }
        }
        else break;
    }
    while (true);



    if (response == "yok") response = "";

    return response;
}


async Task<allProducts[]> getProducts()
{
    var t9 = Task.Run(() => GetResponseFromURI("http://127.0.0.1/rest_api"));
    await t9.ConfigureAwait(false);
    return JsonConvert.DeserializeObject<allProducts[]>(t9.Result);
}

allProducts[] ap = getProducts().Result;

foreach (var t in ap)
{
    if (t == null)
    {
        Console.WriteLine("Veriler alınamadı.");
        return;
    }
    else
    {
        Console.WriteLine(t.pn + " verileri başarı ile alındı.");
    }
}


Console.WriteLine("Tüm veriler başarı ile alındı.\nBot başlatılıyor...");



Random chance = new Random();

discord = new DiscordClient(new DiscordConfiguration
{
    Token = "x.x.x",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.All
});

discord.MessageCreated += async (s, e) =>
{
    ArrayList RandomGameItems = new ArrayList();
    ArrayList RandomGameItemsPrices = new ArrayList();
    ArrayList RandomGameItemsImgUrls = new ArrayList();
    ArrayList RandomGameItemsWinRate = new ArrayList();

    int id = -1;
    string tur = "\\-", gameImgUrl = "", prodObjs = "";

    foreach (var t in ap)
    {
        if (e.Message.Content.ToLower().StartsWith(t.pn.ToLower()))
        {
            id = t.pi;
            tur = t.pn;
            gameImgUrl = "https://127.0.0.1/" + t.pm;
            prodObjs = t.product;
        }
    }

    TBL[] productObjects = JsonConvert.DeserializeObject<TBL[]>(prodObjs.Replace('--', '"'));

    if (productObjects != null)
    {
        for (int i = 0; i < productObjects.Length; i++)
        {
            if (productObjects[i].pn == "0")
            {
                productObjects[i].pn = "A Kiss :kiss: from The Devil :smiling_imp:";
            }

            RandomGameItems.Add(productObjects[i].pn);
            RandomGameItemsPrices.Add(productObjects[i].pp);
            RandomGameItemsImgUrls.Add(productObjects[i].pm);
            RandomGameItemsWinRate.Add(productObjects[i].pr);
        }
    }

    if (e.Message.Content.ToLower().StartsWith(tur.ToLower() + " random"))
    {
        ArrayList finalArray = new ArrayList();
        ArrayList finalWinRateArray = new ArrayList();
        ArrayList finalPrizeArray = new ArrayList();
        ArrayList finalImgUrlArray = new ArrayList();

        for (int i = 0; i < RandomGameItems.Count; i++)
        {
            int itemWinRate = (int)RandomGameItemsWinRate[i];

            if (itemWinRate != 0)
            {
                for (int i2 = 0; i2 < itemWinRate; i2++)
                {
                    finalArray.Add(RandomGameItems[i]);
                    finalWinRateArray.Add(itemWinRate);
                    finalPrizeArray.Add(RandomGameItemsPrices[i]);
                    finalImgUrlArray.Add(RandomGameItemsImgUrls[i]);
                }
            }
        }


        int c = chance.Next(0, finalArray.Count);

        string reward = finalArray[c].ToString();

        int winRate = Convert.ToInt32(finalWinRateArray[c]);

        double prize = Convert.ToDouble(finalPrizeArray[c]);

        string rewardEmoji = "";

        string imgUrl = finalImgUrlArray[c].ToString();


        if (prize <= 5)
        {
            rewardEmoji = ":poop:";
        }
        else if (prize <= 7)
        {
            rewardEmoji = ":coin:";
        }
        else if (prize <= 9)
        {
            rewardEmoji = ":money_with_wings:";
        }
        else if (prize == 10)
        {
            rewardEmoji = ":champagne_glass:";
        }
        else if (prize <= 12)
        {
            rewardEmoji = ":money_mouth:";
        }
        else
        {
            rewardEmoji = ":moneybag:";
        }

        rewardEmoji = string.Concat(Enumerable.Repeat(rewardEmoji, 3));

        await e.Message.RespondAsync(embed: new DiscordEmbedBuilder
        {
            Title = "Special " + tur + " Random Game",
            Description = ":partying_face: :confetti_ball: ***CONGRATULATIONS! " + e.Author.Mention + " :confetti_ball: :partying_face: ***\n\n*YOU HAVE WON:*\n\n> " + rewardEmoji + " **" + reward + "** $" + prize + " " + rewardEmoji + "\n\n:clap_tone3: :clap_tone3: :clap_tone3: :clap_tone3: :clap_tone3: :clap_tone3:\n\n:point_right::skin-tone-3: Your chance number: **" + c + "** :four_leaf_clover: in **" + finalArray.Count + "** :man_mage::skin-tone-3:, Win Rate: " + winRate + "% :nazar_amulet: -- game by **MuMMy**",
            ImageUrl = "https://127.0.0.1/" + imgUrl,
            Color = DiscordColor.DarkGreen,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = gameImgUrl
            }
        });

    }
    else if (tur != "\\-")
    {
        ArrayList finalArray = new ArrayList();
        ArrayList finalWinRateArray = new ArrayList();
        ArrayList finalPrizeArray = new ArrayList();

        ArrayList finalPossibility = new ArrayList();

        string b1 = "", b2 = "", b3 = "", b4 = "", b5 = "", b6 = "", b7 = "", b8 = "", b9 = "";

        b1 = "**Reward" + " - " + "Price" + " - " + "Win Rate**\n\n";

        for (int i = 0; i < RandomGameItems.Count; i++)
        {
            int itemWinRate = (int)RandomGameItemsWinRate[i];

            if (itemWinRate != 0)
            {
                finalArray.Add(RandomGameItems[i]);
                finalWinRateArray.Add(itemWinRate);
                finalPrizeArray.Add(RandomGameItemsPrices[i]);

                for (int i2 = 0; i2 < itemWinRate; i2++)
                {
                    finalPossibility.Add(itemWinRate);
                }

                string x = "**" + RandomGameItems[i] + "** - $" + RandomGameItemsPrices[i] + " - " + itemWinRate + "%\n";

                if (i <= 8)
                {
                    b1 += x;
                }
                if (i > 8 && i <= 16)
                {
                    b2 += x;
                }
                if (i > 16 && i <= 24)
                {
                    b3 += x;
                }
                if (i > 24 && i <= 32)
                {
                    b4 += x;
                }
                if (i > 32 && i <= 40)
                {
                    b5 += x;
                }
                if (i > 40 && i <= 48)
                {
                    b6 += x;
                }
                if (i > 48 && i <= 56)
                {
                    b7 += x;
                }
                if (i > 56 && i <= 64)
                {
                    b8 += x;
                }
                if (i > 64 && i <= 72)
                {
                    b9 += x;
                }
            }
        }

        async Task<DiscordMessage> z(string v)
        {
            if (v != "")
                return await e.Message.RespondAsync(v);
            else return null;
        }

        await z("**Special " + tur + " Random Game**\n\n" + b1);
        await z(b2);
        await z(b3);
        await z(b4);
        await z(b5);
        await z(b6);
        await z(b7);
        await z(b8);
        await z(b9);
        await z("\nTotal Possibility = **" + finalPossibility.Count + "** -- game by **MuMMy**");
    }


};



await discord.ConnectAsync();
Console.WriteLine("Bot başarı ile başlatıldı! by MuMMy");
await Task.Delay(-1);