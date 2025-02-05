﻿using System.Collections.Generic;
using System.Linq;
using Mirror;
using SS3D.Content.Systems.Interactions;
using SS3D.Engine.Interactions;
using SS3D.Engine.Interactions.Extensions;
using SS3D.Engine.Inventory;
using UnityEngine;

namespace SS3D.Content.Items.Cosmetic
{
    public class ServiceBell : Item, IInteractionTarget
    {
        public Sprite interactionIcon;

        private class BellInteraction : IInteraction
        {
            public IClientInteraction CreateClient(InteractionEvent interactionEvent)
            {
                return null;
            }

            public string GetName(InteractionEvent interactionEvent)
            {
                return "Bell";
            }

            public Sprite GetIcon(InteractionEvent interactionEvent)
            {
                if (interactionEvent.Target is ServiceBell bell)
                    return bell.interactionIcon;
                if (interactionEvent.Source is ServiceBell bell1)
                    return bell1.interactionIcon;
                return null;
            }

            public bool CanInteract(InteractionEvent interactionEvent)
            {
                if (interactionEvent.Target is ServiceBell bell)
                {
                    if (!InteractionExtensions.RangeCheck(interactionEvent))
                    {
                        return false;
                    }
                    return true;
                }

                return false;
            }

            public bool Start(InteractionEvent interactionEvent, InteractionReference reference)
            {
                if (interactionEvent.Target is ServiceBell bell)
                {
                    bell.Bell();
                }
                return false;
            }

            public bool Update(InteractionEvent interactionEvent, InteractionReference reference)
            {
                throw new System.NotImplementedException();
            }

            public void Cancel(InteractionEvent interactionEvent, InteractionReference reference)
            {
                throw new System.NotImplementedException();
            }
        }
        
        [SerializeField] private AudioClip bellSound = null;

        public override void Start()
        {
            base.Start();
            GenerateNewIcon(); 
        }

        [Server]
        private void Bell()
        {
            AudioManager.Instance.PlayAudioSource(bellSound, gameObject);
        }

        [ClientRpc]
        private void RpcPlayBell()
        {
            AudioManager.Instance.PlayAudioSource(bellSound, gameObject);
        }

        public override IInteraction[] GenerateInteractionsFromTarget(InteractionEvent interactionEvent)
        {
            List<IInteraction> interactions = base.GenerateInteractionsFromTarget(interactionEvent).ToList();
            interactions.Insert(0, new BellInteraction());
            return interactions.ToArray();
        }
    }
}