using System.Collections;
using FinTOKMAK.SkillSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SkillLogicManagerTest
{
    // A Test behaves as an ordinary method
    //[Test]
    //public void NewTestScriptSimplePasses()
    //{
    //    // Use the Assert class to test conditions

    //}

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    private GameObject _gameObject;
    private TestSkillLogic _logic;
    private SkillLogicManager _logicManager;

    [SetUp]
    public void Init()
    {
        _gameObject = new GameObject();
        _logicManager = _gameObject.AddComponent<SkillLogicManager>();
        _logic = new TestSkillLogic();
        _logic.continueDeltaTime = 0.02f;
        _logic.effectType = SkillEffectType.ARMode;
        _logic.continueStopTimeOverlay = true;
        _logic.continueTime = 1f;
        _logic.id = "Logic";
    }

    [TearDown]
    public void Destroy()
    {
        Object.Destroy(_gameObject);
        _logicManager = null;
        _logic = null;
    }

    [UnityTest]
    public IEnumerator SkillLogic_Manager_Test_Add()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        Assert.IsFalse(_logic.runAdd);
        _logicManager.Add(_logic);
        Assert.IsTrue(_logic.runAdd);
        yield return null;
    }

    /// <summary>
    ///     主动调用Remove方法来结束技能
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator SkillLogic_Manager_Test_Remove()
    {
        //将技能加入skillLogic
        _logicManager.Add(_logic);
        //检查确认技能的OnRemove方法未执行
        Assert.IsFalse(_logic.runRemove);
        //调用Remove方法移除技能
        _logicManager.Remove(_logic.id);
        //检查移除技能触发的OnRemove方法是否执行了
        Assert.IsTrue(_logic.runRemove);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SkillLogic_Manager_Test_AutoRemove()
    {
        //设置技能持续时间为1秒
        _logic.continueTime = 1f;
        //将技能加入skillLogic
        _logicManager.Add(_logic);
        //检查确认技能的OnRemove方法未执行
        Assert.IsFalse(_logic.runRemove);
        yield return new WaitForSeconds(0.5f);
        //等待0.5秒，检查是否提前执行了OnRemove
        Assert.IsFalse(_logic.runRemove);
        //等待2秒后，查看是否自动OnRemove技能
        yield return new WaitForSeconds(2);
        Assert.IsTrue(_logic.runRemove);
    }

    [UnityTest]
    public IEnumerator SkillLogic_Manager_Test_Continue()
    {
        //设置技能持续时间为1秒
        _logic.continueTime = 1f;
        //设置技能持续执行间隔为0.1秒
        _logic.continueDeltaTime = 0.1f;
        //将技能设置为ARC模式,这样会执行Add,Remove和Continue方法
        _logic.effectType = SkillEffectType.ARContinueMode;
        //检查Continue方法未执行
        Assert.IsFalse(_logic.runContinue);
        //将技能添加进去
        _logicManager.Add(_logic);
        //再次检查确认Continue方法未执行
        Assert.IsFalse(_logic.runContinue);
        //等待2秒
        yield return new WaitForSeconds(2);
        //检查确认Continue方法已经执行
        Assert.IsTrue(_logic.runContinue);
    }

    /// <summary>
    ///     技能触发后的持续触发间隔检测
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator SkillLogic_Manager_Test_ContinueRunDelta()
    {
        //设置技能持续时间为1秒
        _logic.continueTime = 1f;
        //设置技能持续执行间隔为0.2秒
        _logic.continueDeltaTime = 0.2f;
        //将技能设置为ARC模式,这样会执行Add,Remove和Continue方法
        _logic.effectType = SkillEffectType.ARContinueMode;
        //将技能添加进去
        _logicManager.Add(_logic);
        //等待2秒
        yield return new WaitForSeconds(2);
        var count = (int)(_logic.continueTime / _logic.continueDeltaTime);
        //检查确认Continue方法的执行次数是否有误
        Assert.IsTrue(_logic.runContinueCount == count);
    }


    public class TestSkillLogic : SkillLogic
    {
        public bool runAdd;
        public bool runRemove;
        public bool runContinue;
        public int runContinueCount;

        public override void OnAdd(SkillLogicManager targer, SkillLogic self)
        {
            base.OnAdd(targer, self);
            runAdd = true;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            runRemove = true;
        }

        public override void OnContinue()
        {
            base.OnContinue();
            runContinue = true;
            runContinueCount++;
            Debug.Log($"ContinueCoun:{runContinueCount}");
        }
    }
}