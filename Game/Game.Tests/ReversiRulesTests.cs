using GameLogic;
using GameLogic.Reversi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GameLogic.TileType;

namespace Game.Tests
{
    [TestClass]
    public class ReversiRulesTests
    {
        [TestMethod]
        public void Test_HasLegalMoves_10x10_True()
        {
            //Arrange
            const int dim = 10;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player2, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var result = rules.GetFirstLegalMove(Player1);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Test_HasLegalMoves_10x10_False()
        {
            //Arrange
            const int dim = 10;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0        1      2      3      4      5      6      7      8      9
                    /*0*/{ Player2, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Player1, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },//6*/
                    /*7*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },//7*/
                    /*8*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty,   Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var result = rules.GetFirstLegalMove(Player1);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Test_HasLegalMoves_6x6_False()
        {
            //Arrange
            const int dim = 6;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0        1        2        3        4      5
                    /*0*/{ Player2, Player2, Player2, Player1, Empty, Empty },/*0*/
                    /*1*/{ Player2, Player2, Player1, Player1, Empty, Empty },/*1*/
                    /*2*/{ Player2, Player2, Empty,   Empty,   Empty, Empty },/*2*/
                    /*3*/{ Player1, Player2, Player1, Empty,   Empty, Empty },/*3*/
                    /*4*/{ Empty,   Player1, Empty,   Empty,   Empty, Empty },/*4*/
                    /*5*/{ Player1, Player1, Player1, Empty,   Empty, Empty }/*5*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var result = rules.GetFirstLegalMove(Player1);

            //Assert
            Assert.IsNull(result);
        }




        [TestMethod]
        public void Test_MapChanges_10x10_True()
        {
            //Arrange
            const int dim = 10;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player2, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var mapAfterMove = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Player1, Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player1, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var changes = rules.GetMapChanges(new Point(3, 5), Player1);
            foreach (var change in changes)
            {
                map.Tiles[change.Point.Row, change.Point.Col] = change.PointType;
            }

            //Assert
            Assert.AreEqual(mapAfterMove, map);
        }

        [TestMethod]
        public void Test_MapChanges_10x10_False()
        {
            //Arrange
            const int dim = 10;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player2, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var mapAfterMove = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player2, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Player1, Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var changes = rules.GetMapChanges(new Point(6, 4), Player1);
            foreach (var change in changes)
            {
                map.Tiles[change.Point.Row, change.Point.Col] = change.PointType;
            }

            //Assert
            Assert.AreNotEqual(mapAfterMove, map);
        }

        [TestMethod]
        public void Test_LegalMove_10x10_True()
        {
            //Arrange
            const int dim = 10;
            var map = new MapData
            {
                Height = dim,
                Width = dim,
                Tiles = new TileType[dim, dim]
                {
                    //       0      1      2      3      4        5        6      7      8      9
                    /*0*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*0*/
                    /*1*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*1*/
                    /*2*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*2*/
                    /*3*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*3*/
                    /*4*/{ Empty, Empty, Empty, Empty, Player1, Player2, Empty, Empty, Empty, Empty },/*4*/
                    /*5*/{ Empty, Empty, Empty, Empty, Player2, Player1, Empty, Empty, Empty, Empty },/*5*/
                    /*6*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*6*/
                    /*7*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*7*/
                    /*8*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty },/*8*/
                    /*9*/{ Empty, Empty, Empty, Empty, Empty,   Empty,   Empty, Empty, Empty, Empty } /*9*/
                }
            };

            var rules = new ReversiRules(map);

            //Act
            var move = rules.GetLegalMove(Player1, 1);


            //Assert
            Assert.AreEqual(new Point(4, 6), move);
        }
    }
}
