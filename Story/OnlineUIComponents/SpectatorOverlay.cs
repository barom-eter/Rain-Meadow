﻿using Menu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public class SpectatorOverlay : Menu.Menu
    {
        public AbstractCreature? spectatee;

        public Vector2 pos;

        public class PlayerButton
        {
            public OnlinePhysicalObject player;
            public SimplerButton button;
            public SimplerSymbolButton? kickbutton;
            public bool mutePlayer
            {
                get => OnlineManager.lobby.gameMode.mutedPlayers.Contains(player.owner.id.name);
                set
                {
                    var name = player.owner.id.name;
                    if (value)
                    {
                        RainMeadow.Debug($"Added {name} to mute list");
                        OnlineManager.lobby.gameMode.mutedPlayers.Add(name);
                    }
                    else
                    {
                        RainMeadow.Debug($"Removed {name} from mute list");
                        OnlineManager.lobby.gameMode.mutedPlayers.Remove(name);
                    }
                }
            }
            private string clientMuteSymbol => mutePlayer ? "FriendA" : "Menu_Symbol_Clear_All";
            public Vector2 pos
            {
                set
                {
                    button.pos = value;
                    if (kickbutton != null)
                        kickbutton.pos = value + new Vector2(120, 0);
                }
            }
            public SpectatorOverlay overlay;

            public PlayerButton(SpectatorOverlay menu, OnlinePhysicalObject opo, Vector2 pos, bool canKick = false)
            {
                this.overlay = menu;
                this.player = opo;
                this.button = new SimplerButton(menu, menu.pages[0], opo.owner.id.name, pos, new Vector2(110, 30));

                this.button.OnClick += (_) =>
                {
                    this.button.toggled ^= true;
                    overlay.spectatee = this.button.toggled ? this.player.apo as AbstractCreature : null;
                };
                this.button.owner.subObjects.Add(button);
                if (canKick)
                {
                    this.kickbutton = new SimplerSymbolButton(menu, menu.pages[0], "Menu_Symbol_Clear_All", "KICKPLAYER", pos + new Vector2(120, 0));
                    this.kickbutton.OnClick += (_) => BanHammer.BanUser(opo.owner);
                    this.kickbutton.owner.subObjects.Add(kickbutton);
                }
                if (opo.owner != OnlineManager.mePlayer)
                {
                    this.kickbutton = new SimplerSymbolButton(menu, menu.pages[0], clientMuteSymbol, "MUTEPLAYER", pos + new Vector2(120, 0));
                    this.kickbutton.OnClick += (_) =>
                    {
                        mutePlayer ^= true;
                        this.kickbutton.UpdateSymbol(clientMuteSymbol);
                    };
                    this.kickbutton.owner.subObjects.Add(kickbutton);
                }
                this.pos = pos;
            }

            public void Destroy()
            {
                this.button.RemoveSprites();
                this.button.page.RemoveSubObject(this.button);
                if (this.kickbutton != null)
                {
                    this.kickbutton.RemoveSprites();
                    this.kickbutton.page.RemoveSubObject(this.kickbutton);
                }
            }
        }

        public RainWorldGame game;
        public List<PlayerButton> playerButtons;

        public SpectatorOverlay(ProcessManager manager, RainWorldGame game) : base(manager, RainMeadow.Ext_ProcessID.SpectatorMode)
        {
            this.game = game;
            this.pages.Add(new Page(this, null, "spectator", 0));
            this.selectedObject = null;
            this.playerButtons = new();
            this.pos = new Vector2(1180, 553);
            this.pages[0].subObjects.Add(new Menu.MenuLabel(this, this.pages[0], this.Translate("PLAYERS"), this.pos, new(110, 30), true));
        }

        private bool UpdateList()
        {
            List<OnlinePhysicalObject> newPlayers = OnlineManager.lobby.playerAvatars
                .Select(kv => kv.Value.FindEntity(true))
                .OfType<OnlinePhysicalObject>()
                .OrderBy(opo => opo.isMine ? 0 : 1)
                .ToList();

            if (newPlayers.Count == playerButtons.Count) return false; // race condition that will Never Happen(TM)

            var offset = new Vector2(0, 38);
            var pos = this.pos - offset;

            for (var i = playerButtons.Count - 1; i >= 0; i--)
            {
                var button = playerButtons[i];
                if (newPlayers.Contains(button.player))
                {
                    newPlayers.Remove(button.player);
                    button.pos = pos;
                    pos -= offset;
                }
                else
                {
                    button.Destroy();
                    playerButtons.RemoveAt(i);
                }
            }

            foreach (var player in newPlayers)
            {
                playerButtons.Add(new PlayerButton(this, player, pos, OnlineManager.lobby.isOwner && !player.isMine));
                pos -= offset;
            }

            return true;
        }

        public override void Update()
        {
            base.Update();
            UpdateList();

            foreach (var button in playerButtons)
            {
                var ac = button.player.apo as AbstractCreature;
                button.button.toggled = ac != null && ac == spectatee;
                button.button.buttonBehav.greyedOut = ac is null || (ac.state.dead || (ac.realizedCreature != null && ac.realizedCreature.State.dead));
            }
        }
    }
}
