using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

class ModData
{

    //Create a ModData class which allows us to store arbitrary data.
    // We can then add this data to the current save with:
    // var model = this.Helper.Data.ReadSaveData<ModData>("example-key");
    // save data (if needed)
    //this.Helper.Data.WriteGlobalData("example-key", model);

    public int previous_bedtime { get; set; }

    public ModData(int bedtime)
    {
        this.previous_bedtime = bedtime;
    }
}

namespace RealisticBedtime
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.TimeChanged += this.whenTimeChange;
            helper.Events.GameLoop.DayStarted += this.whenDayStarted;
            helper.Events.GameLoop.DayEnding += this.whenDayEnded;
            helper.Events.GameLoop.SaveLoaded += this.whenSaveLoaded;
        }

        /*****
        ** Private variables
        ******/

        private int timeDayEnded = 600;
        
        /*********
        ** Private methods
        *********/
        private void whenSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Our task in this event is to reload the previous day end time.

            ModData model = new ModData(600); // this will call the model constructor, sets the previous_bedtime to 600.

            // Reads the saveed data. If this is the 1st day, previous_bedtime is set to 600. Otherwise, should be correct.
            var previousData = this.Helper.Data.ReadSaveData<ModData>("previous_bedtime");

            if (previousData == null) // if we didn't read any data, then the data doesn't exist! We need to make some data to read:
            {

                this.Helper.Data.WriteSaveData("previous_bedtime", model); // creates the data
                previousData = this.Helper.Data.ReadSaveData<ModData>("previous_bedtime"); // re-reads data now that it exists.
            }

            timeDayEnded = previousData.previous_bedtime; //sets the private variable to timeDayEnded to the previous day's bedtime.
            // It works! The game now remembers your previous bedtime!

            // NOTE: MULTIPLAYER IS UNTESTED BUT LIKELY ONLY WORKS FOR THE MAIN PLAYER!
            // In multiplayer other farmers probably will either start every fresh play session with energized, or be totally unaffected by the mod.
        
        }


        private void whenTimeChange(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime == 2210) {
                Game1.addHUDMessage(new HUDMessage("You are feeling tired...", 3));
            }
            else if (e.NewTime == 2410)
            {
                Game1.addHUDMessage(new HUDMessage("You are feeling very tired...", 3));
            }
            else if (e.NewTime == 2510)
            {
                Game1.addHUDMessage(new HUDMessage("You are feeling exhausted!", 3));
            }  
        }

        private void whenDayStarted(object sender, DayStartedEventArgs e)
        {
            // Define new buffs that can be applied at the beginning of the day.
            var energized = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 15, "speed", "Energized!")
            {
                which = 928880,
                millisecondsDuration = 900000
            };

            var tired = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 15, "speed", "Tired")
            {
                which = 928883,
                millisecondsDuration = 900000
            };

            var veryTired = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -2, 0, 0, 15, "speed", "Very Tired")
            {
                which = 928883,
                millisecondsDuration = 900000
            };

            var exhausted = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -3, 0, 0, 15, "speed", "Exhausted!")
            {
                which = 928883,
                millisecondsDuration = 900000
            };
            
            var nonFunctional = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, -10, 0, 0, 15, "speed", "Non-functional...")
            {
                which = 928883,
                millisecondsDuration = 900000
            };

            if (timeDayEnded <= 2200) // Sleep before or at 10:00 pm, you are energized!
            {
                Game1.buffsDisplay.addOtherBuff(energized);
            }

            else if (timeDayEnded > 2200 && timeDayEnded <= 2400) // Between 10:10pm and 12:00 am, you are tired!
            {
                Game1.player.Stamina = (Game1.player.Stamina / 2);
                Game1.buffsDisplay.addOtherBuff(tired);
            }
            else if (timeDayEnded > 2400 && timeDayEnded <= 2500) // Between 12:10 and 1:00 am, you are very tired!
            {
                Game1.player.Stamina = (Game1.player.Stamina / 3);
                Game1.buffsDisplay.addOtherBuff(veryTired);
            }
            else if (timeDayEnded > 2500 && timeDayEnded <= 2550) // Between 1:10 am and 1:50 am, you are exhausted!
            {
                Game1.player.Stamina = (Game1.player.Stamina / 4);
                Game1.buffsDisplay.addOtherBuff(exhausted);
            }
            else // Pass out at 2am, you are non-functional!
            {
                Game1.player.Stamina = (Game1.player.Stamina * 0);
                Game1.buffsDisplay.addOtherBuff(nonFunctional);
        } 

        private void whenDayEnded(object sender, DayEndingEventArgs e)
        {
            timeDayEnded = Game1.timeOfDay;

            ModData model = new ModData(timeDayEnded); // this will call the model constructor when code loads, sets the previous_bedtime to timeDayEnded and bool to true.

            // read in the previous day's bedtime first
            //model = this.Helper.Data.ReadSaveData<ModData>("previous_bedtime");

            //model.previous_bedtime = timeDayEnded; // set the model's previous bedtime to the correct bedtime.
            //model.isDataLoaded = true; //just a test function for now.

            // Then overwrite the save data with the new bedtime.
            this.Helper.Data.WriteSaveData("previous_bedtime", model);
        }
    }
}
