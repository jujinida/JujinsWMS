using System;//System네임스페이스사용.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1//네임스페이스선언.
//공간 선언을 통해 중복되는 이름을 피할 수 있다. EX)ConsoleApp1.Apple클래스 ConsoleApp2.Apple클래스
{
    //class는 하나의 객체를 표현한다. 
    //Ex)자동차(class car)가있으면 자동차의 구성요소(맴버변수)와 행동적인요소(method)를 가질 수 있다.
    class Car
    {
        private string handle = "AMODEL";

        public void carRun()
        {
            Console.WriteLine("자동차를 움직인다.");
        }
    }

    //class Program은 C#에서 프로그램의 가장 첫번째 실행되는 Main메소드를 포함하는 용도로 주로 사용한다.
    class Program
    {
        //프로그램의 주시작점.
        static void Main(string[] args)
        {
            //System네임스페이스의 Console클래스의 WriteLine메소드 호출
            System.Console.WriteLine("Syste을 이용한 호출");
            Console.WriteLine("Hello World!");
        }
    }
}