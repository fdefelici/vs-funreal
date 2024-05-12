using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System;


namespace FUnrealTest
{

    public class Pod
    {
        public string fstring = null;
        public int fint = -1;
        public float ffloat = -1;
        public bool fbool = false;
        public PodStruct fstruct;
        //array non usato realmente ma puo servire
    }

    public struct PodStruct
    {

        public PodStruct(int _x)
        {
            x = _x;
        }

        public int x;
    }

    public class Vector2
    {
        public static Vector2 Zero = new Vector2(0, 0);
        public static Vector2 One = new Vector2(1, 1);
        public float x;
        public float y;

        public Vector2()
        {
            x = 0; y = 0;
        }
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"( x: {x}, y: {y} )";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2)) return false;
            var other = obj as Vector2;
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
    }

    public class PodPermissive
    {
        public string fstring = null;
        public Vector2 fvec2 = Vector2.Zero;
    }

    public class PodArray
    {
        public int[] ints = null;
        public string[] strings = null;
    }

    [Serializable]
    class Container
    {
        public Internal intern1 = null;
        public Internal intern2 = null;
    }

    [Serializable]
    class Internal
    {
        public string name = null;
    }

    [TestClass]
    public class XJsonUtilsTest
    {

        [TestMethod]
        public void CustomJsonParser_SimpleFields()
        {
            bool result;

            result = XJsonUtils.TryFromJsonString<Pod>("", out var pod01);
            Assert.IsFalse(result);
            Assert.IsNull(pod01);

            result = XJsonUtils.TryFromJsonString<Pod>("{}", out var pod02);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod02);
            Assert.AreEqual(null, pod02.fstring);
            Assert.AreEqual(-1, pod02.fint);
            Assert.AreEqual(-1, pod02.ffloat);
            Assert.AreEqual(false, pod02.fbool);
            Assert.AreEqual(0, pod02.fstruct.x);

            string json;

            json = @"
        {
            ""fstring"": ""hello"",
            ""fint"" : 10,
            ""ffloat"" : 0.5,
            ""fbool"" : true,
            ""fstruct"": { ""x"" : 1}
        }";
            result = XJsonUtils.TryFromJsonString<Pod>(json, out var pod03);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod03);
            Assert.AreEqual("hello", pod03.fstring);
            Assert.AreEqual(10, pod03.fint);
            Assert.AreEqual(0.5f, pod03.ffloat);
            Assert.AreEqual(true, pod03.fbool);
            Assert.AreEqual(1, pod03.fstruct.x);
        }

        [TestMethod]
        public void InternalClassAreAlwaysInstantiatedCustom()
        {
            bool result;
            result = XJsonUtils.TryFromJsonString<Container>("{}", out var cont01);
            Assert.IsTrue(result);
            Assert.IsNotNull(cont01);
            Assert.IsNull(cont01.intern1);
            Assert.IsNull(cont01.intern2);

            string json;

            json = @"
        {
            ""intern2"": { ""name"" : ""intern2"" }
        }";
            result = XJsonUtils.TryFromJsonString<Container>(json, out var cont02);
            Assert.IsTrue(result);
            Assert.IsNotNull(cont02);
            Assert.IsNull(cont02.intern1);
            Assert.AreEqual("intern2", cont02.intern2.name);
        }

        [TestMethod]
        public void CustomJsonParser_Permissive()
        {
            bool result;

            string json;

            json = @"
        {
            ""fstring"": 1,
            ""fvec2"": { ""x"" : 1, ""y"" : 1 }
        }";
            result = XJsonUtils.TryFromJsonString<PodPermissive>(json, true, out var pod);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod);
            Assert.AreEqual("1", pod.fstring);
            Assert.AreEqual(Vector2.One, pod.fvec2);
        }

        [TestMethod]
        public void CustomJsonParser_PermissiveWithObjectToString()
        {
            bool result;

            string json;

            json = @"
        {
            ""fstring"": { ""x"" : 1, ""y"" : 1 },
            ""fvec2"": { ""x"" : 1, ""y"" : 1 }
        }";
            result = XJsonUtils.TryFromJsonString<PodPermissive>(json, true, out var pod);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod);
            Assert.AreEqual("{\"x\":1,\"y\":1}", pod.fstring);
            Assert.AreEqual(Vector2.One, pod.fvec2);
        }

        [TestMethod]
        public void CustomJsonParser_PermissiveWithArrayToString()
        {
            bool result;

            string json;

            json = @"
        {
            ""fstring"": [{ ""p"" : 1 }, { ""p"" : 2 }],
            ""fvec2"": { ""x"" : 1, ""y"" : 1 }
        }";
            result = XJsonUtils.TryFromJsonString<PodPermissive>(json, true, out var pod);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod);
            Assert.AreEqual("[{\"p\":1},{\"p\":2}]", pod.fstring);
            Assert.AreEqual(Vector2.One, pod.fvec2);
        }

        [TestMethod]
        public void CustomJsonParser_Array_Int()
        {
            bool result;

            string json;

            json = @"
        {
            ""ints"": [ 1, 2, 3 ]
        }";
            result = XJsonUtils.TryFromJsonString<PodArray>(json, true, out var pod);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod);

            Assert.AreEqual(3, pod.ints.Length);
            Assert.AreEqual(1, pod.ints[0]);
            Assert.AreEqual(2, pod.ints[1]);
            Assert.AreEqual(3, pod.ints[2]);
        }

        [TestMethod]
        public void CustomJsonParser_Array_String()
        {
            bool result;

            string json;

            json = @"
        {
            ""strings"": [ ""a1"", ""a2"", ""a3"" ]
        }";
            result = XJsonUtils.TryFromJsonString<PodArray>(json, true, out var pod);
            Assert.IsTrue(result);
            Assert.IsNotNull(pod);

            Assert.AreEqual(3, pod.strings.Length);
            Assert.AreEqual("a1", pod.strings[0]);
            Assert.AreEqual("a2", pod.strings[1]);
            Assert.AreEqual("a3", pod.strings[2]);
        }
    }
}
