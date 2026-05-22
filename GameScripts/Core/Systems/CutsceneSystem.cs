using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutsceneSystem
{
    private WorldManager _world;

    public List<QueuedAnimation> queuedAnimations = new List<QueuedAnimation>();
    public QueuedAnimation currentAnimation;
    public Coroutine currentAnimationRoutine;

    public CutsceneSystem(WorldManager world)
    {
        _world = world;
    }

    public bool InAnimation => currentAnimationRoutine != null || currentAnimation != null;

    public void QueueAnimation(QueuedAnimation anim)
    {
        queuedAnimations.Add(anim);
    }

    public void CheckQueuedAnimations()
    {
        if (InAnimation)
        {
            return;
        }
        if (GameCanvas.instance.ModalIsOpen)
        {
            return;
        }
        if (!_world.IsPlaying)
        {
            return;
        }
        if (currentAnimation == null && queuedAnimations.Count > 0)
        {
            _world.CloseOpenInventories();
            _world.SetViewType(ViewType.Default);
            currentAnimation = queuedAnimations[0];
            currentAnimation.OnActivate();
            queuedAnimations.RemoveAt(0);
        }
    }

    public void QueueCutscene(IEnumerator coroutine)
    {
        QueueAnimation(new QueuedAnimation(delegate
        {
            _world.StartCoroutine(coroutine);
        }, null));
    }

    public void QueueCutsceneIfNotQueued(IEnumerator coroutine, string id)
    {
        if (queuedAnimations.Any<QueuedAnimation>((QueuedAnimation x) => x.Id == id))
        {
            return;
        }
        QueueAnimation(new QueuedAnimation(delegate
        {
            _world.StartCoroutine(coroutine);
        }, id));
    }

    public void QueueCutsceneIfNotPlayed(string cutsceneId)
    {
        if (_world.CurrentRunVariables.PlayedCutsceneIds.Contains(cutsceneId))
        {
            return;
        }
        _world.CurrentRunVariables.PlayedCutsceneIds.Add(cutsceneId);
        QueueCutscene(cutsceneId);
    }

    public void QueueCutscene(string cutsceneId)
    {
        ScriptableCutscene cutsceneWithId = _world.GameDataLoader.GetCutsceneWithId(cutsceneId);
        QueueCutscene(cutsceneWithId);
    }

    public void QueueCutscene(ScriptableCutscene cutscene)
    {
        QueueAnimation(new QueuedAnimation(delegate
        {
            if (!_world.CurrentRunVariables.PlayedCutsceneIds.Contains(cutscene.CutsceneId))
            {
                _world.CurrentRunVariables.PlayedCutsceneIds.Add(cutscene.CutsceneId);
            }
            _world.StartCoroutine(Cutscenes.RunScriptableCutscene(cutscene));
        }, null));
    }
}
