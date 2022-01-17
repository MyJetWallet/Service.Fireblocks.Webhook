﻿using System;
using NUnit.Framework;
using Service.Fireblocks.Webhook.Services;

namespace Service.Circle.Webhooks.Tests
{
    public class CryptoProviderTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CorrectSignaturePasses()
        {
            var signature = Convert.FromBase64String("XuTiwzE6VOThjAO+hG7E97cfRdH+8JCXv3iLKlQhzgXNRk/RUP07ZBe0/NoXhLQrNTXqpgkghhs//oKSPSJU7uO2beEfynZgfnlAxHUY2a5zwk1eZMWY12lp2R72gBcracori8sajnegJpRI7X6ukKnABSxwKVFsz1jwzOmU3rdtLZyirJ0Wum3L9U7GIRKlZ0beVplBfhqBQVpNwNPZE6dfwwkKlebOXtTMb0g/N7YpWuHEAZs7Y/IesE+FuAH4WNfNYaEUxbDGsL9BMWAYyKrRQEiXoRmq2ZByzZFMJtvSgGgWMcHJwumxalyoW15I7yo5XmR4sRIHxNNIV5qbOABohblNJhm07dQeRnjCCZxqt3AXT6kdgdn1jkcC4wP9bOMfc/+CbBNWeQvw2p4oxiOPvEPKXC6KJEmqXpCPjnsGC6gRfzutmvybAuDqvHDg2UGCSrjt6uD3SNX+BZ+F8m478q+NlGo5JyFTny5vaqrO+ikwWYe/drBCnOVmn90101Km+pSM0qn9wC5nFAeCYV7jW6c0DNtbhKMk6flBbQBLqbzY679J5HBUGmxmBqbNHCaPjk3PyOlM9hBBigmkG9d8Zz1ybjgS62YkGYgH6BAj6To9k92iM56TaOBxZ+TZJPeMMtHdWq0EToesxceZ9iWdcssbo0t2bq7CiQW4xIc=");
            var data = Convert.FromBase64String("eyJ0eXBlIjoiVFJBTlNBQ1RJT05fQ1JFQVRFRCIsInRlbmFudElkIjoiYWI1ODkwNjQtNTZmYy01YzRhLWFhOTUtMWYzMGE2ZDJhODNhIiwidGltZXN0YW1wIjoxNjQxODI0OTEwNjQ4LCJkYXRhIjp7ImlkIjoiMWYwODZmZGEtZWJjYS00NzU5LTlhYWEtYzExMGEzNzI3ODE5IiwiY3JlYXRlZEF0IjoxNjQxODI0ODY0MDAwLCJsYXN0VXBkYXRlZCI6MTY0MTgyNDkwMDEzNSwiYXNzZXRJZCI6IkVUSF9URVNUIiwic291cmNlIjp7ImlkIjoiNTc3YWIwNDktYTY5OS01NTJiLWYzODMtN2NjY2QxYjQ1ZDgwIiwidHlwZSI6IkVYVEVSTkFMX1dBTExFVCIsIm5hbWUiOiIweDFFYWI3ZDQxMmEyNWE1ZDAwRWMzZDA0NjQ4YWE1NENlQTRhQjdlOTQiLCJzdWJUeXBlIjoiRXh0ZXJuYWwifSwiZGVzdGluYXRpb24iOnsiaWQiOiIxMSIsInR5cGUiOiJWQVVMVF9BQ0NPVU5UIiwibmFtZSI6IlNpbXBsZSBXaXRoZHJhd2FsIEFjY291bnQiLCJzdWJUeXBlIjoiIn0sImFtb3VudCI6MC4wMDEsIm5ldHdvcmtGZWUiOjAuMDAwMDQwMzA5OTY2OTc3LCJuZXRBbW91bnQiOjAuMDAxLCJzb3VyY2VBZGRyZXNzIjoiMHgxRWFiN2Q0MTJhMjVhNWQwMEVjM2QwNDY0OGFhNTRDZUE0YUI3ZTk0IiwiZGVzdGluYXRpb25BZGRyZXNzIjoiMHg1MkE2NDM0MjEzYTk5QjAzZkUxZjBDNTlCMzNkOEVDMDg4QTlGQjhEIiwiZGVzdGluYXRpb25BZGRyZXNzRGVzY3JpcHRpb24iOiIiLCJkZXN0aW5hdGlvblRhZyI6IiIsInN0YXR1cyI6IkNPTkZJUk1JTkciLCJ0eEhhc2giOiIweGI2M2I3YmI4NGI4OTE4NTk5ZmM1N2E3Y2RjZDc5NDdhYTU1NzllNjA4ZDM2MjM3ZmI1NWEwOWFjYmUzYjNjYmQiLCJzdWJTdGF0dXMiOiJQRU5ESU5HX0JMT0NLQ0hBSU5fQ09ORklSTUFUSU9OUyIsInNpZ25lZEJ5IjpbXSwiY3JlYXRlZEJ5IjoiIiwicmVqZWN0ZWRCeSI6IiIsImFtb3VudFVTRCI6MywiYWRkcmVzc1R5cGUiOiIiLCJub3RlIjoiIiwiZXhjaGFuZ2VUeElkIjoiIiwicmVxdWVzdGVkQW1vdW50IjowLjAwMSwiZmVlQ3VycmVuY3kiOiJFVEhfVEVTVCIsIm9wZXJhdGlvbiI6IlRSQU5TRkVSIiwiY3VzdG9tZXJSZWZJZCI6bnVsbCwibnVtT2ZDb25maXJtYXRpb25zIjoxLCJhbW91bnRJbmZvIjp7ImFtb3VudCI6IjAuMDAxIiwicmVxdWVzdGVkQW1vdW50IjoiMC4wMDEiLCJuZXRBbW91bnQiOiIwLjAwMSIsImFtb3VudFVTRCI6IjMuMDAifSwiZmVlSW5mbyI6eyJuZXR3b3JrRmVlIjoiMC4wMDAwNDAzMDk5NjY5NzcifSwiZGVzdGluYXRpb25zIjpbXSwiZXh0ZXJuYWxUeElkIjpudWxsLCJibG9ja0luZm8iOnsiYmxvY2tIZWlnaHQiOiIxMTc4OTQ5NSIsImJsb2NrSGFzaCI6IjB4NzI3Njg0ODcxMWEzZDM1ODU3MzYyNDk1YmFlY2VhZGY4N2I1NzViNzdlNzE4NmE0OGFjOTc0M2NlZDQyZTMxNCJ9LCJzaWduZWRNZXNzYWdlcyI6W10sImluZGV4IjowfX0=");

            var isVerified =  CryptoProvider.VerifySignature(data, signature);

            Assert.True(isVerified);
        }

        [Test]
        public void WrongSignatureCantPass()
        {
            var signature = Convert.FromBase64String("XuTiwzE6VOThjAO+hG7E97cfRdH+8JCXv3iLKlQhzgXNRk/RUP07ZBe0/NoXhLQrNTXqpgkghhs//oKSPSJU7uO2beEfynZgfnlAxHUY2a5zwk1eZMWY12lp2R72gBcracori8sajnegJpRI7X6ukKnABSxwKVFsz1jwzOmU3rdtLZyirJ0Wum3L9U7GIRKlZ0beVplBfhqBQVpNwNPZE6dfwwkKlebOXtTMb0g/N7YpWuHEAZs7Y/IesE+FuAH4WNfNYaEUxbDGsL9BMWAYyKrRQEiXoRmq2ZByzZFMJtvSgGgWMcHJwumxalyoW15I7yo5XmR4sRIHxNNIV5qbOABohblNJhm07dQeRnjCCZxqt3AXT6kdgdn1jkcC4wP9bOMfc/+CbBNWeQvw2p4oxiOPvEPKXC6KJEmqXpCPjnsGC6gRfzutmvybAuDqvHDg2UGCSrjt6uD3SNX+BZ+F8m478q+NlGo5JyFTny5vaqrO+ikwWYe/drBCnOVmn90101Km+pSM0qn9wC5nFAeCYV7jW6c0DNtbhKMk6flBbQBLqbzY679J5HBUGmxmBqbNHCaPjk3PyOlM9hBBigmkG9d8Zz1ybjgS62YkGYgH6BAj6To9k92iM56TaOBxZ+TZJPeMMtHdWq0EToesxceZ9iWdcssbo0t2bq7CiQW4xIc=");
            var data = Convert.FromBase64String("eyJ0eXBlIjoiVFJBTlNBQ1RJT05fQ1JFQVRFRCIsInRlbmFudElkIjoiYWI1ODkwNjQtNTZmYy01YzRhLWFhOTUtMWYzMGE2ZDJhODNhIiwidGltZXN0YW1wIjoxNjQxODI0OTEwNjQ4LCJkYXRhIjp7ImlkIjoiMWYwODZmZGEtZWJjYS00NzU5LTlhYWEtYzExMGEzNzI3ODE5IiwiY3JlYXRlZEF0IjoxNjQxODI0ODY0MDAwLCJsYXN0VXBkYXRlZCI6MTY0MTgyNDkwMDEzNSwiYXNzZXRJZCI6IkVUSF9URVNUIiwic291cmNlIjp7ImlkIjoiNTc3YWIwNDktYTY5OS01NTJiLWYzODMtN2NjY2QxYjQ1ZDgwIiwidHlwZSI6IkVYVEVSTkFMX1dBTExFVCIsIm5hbWUiOiIweDFFYWI3ZDQxMmEyNWE1ZDAwRWMzZDA0NjQ4YWE1NENlQTRhQjdlOTQiLCJzdWJUeXBlIjoiRXh0ZXJuYWwifSwiZGVzdGluYXRpb24iOnsiaWQiOiIxMSIsInR5cGUiOiJWQVVMVF9BQ0NPVU5UIiwibmFtZSI6IlNpbXBsZSBXaXRoZHJhd2FsIEFjY291bnQiLCJzdWJUeXBlIjoiIn0sImFtb3VudCI6MC4wMDEsIm5ldHdvcmtGZWUiOjAuMDAwMDQwMzA5OTY2OTc3LCJuZXRBbW91bnQiOjAuMDAxLCJzb3VyY2VBZGRyZXNzIjoiMHgxRWFiN2Q0MTJhMjVhNWQwMEVjM2QwNDY0OGFhNTRDZUE0YUI3ZTk0IiwiZGVzdGluYXRpb25BZGRyZXNzIjoiMHg1MkE2NDM0MjEzYTk5QjAzZkUxZjBDNTlCMzNkOEVDMDg4QTlGQjhEIiwiZGVzdGluYXRpb25BZGRyZXNzRGVzY3JpcHRpb24iOiIiLCJkZXN0aW5hdGlvblRhZyI6IiIsInN0YXR1cyI6IkNPTkZJUk1JTkciLCJ0eEhhc2giOiIweGI2M2I3YmI4NGI4OTE4NTk5ZmM1N2E3Y2RjZDc5NDdhYTU1NzllNjA4ZDM2MjM3ZmI1NWEwOWFjYmUzYjNjYmQiLCJzdWJTdGF0dXMiOiJQRU5ESU5HX0JMT0NLQ0hBSU5fQ09ORklSTUFUSU9OUyIsInNpZ25lZEJ5IjpbXSwiY3JlYXRlZEJ5IjoiIiwicmVqZWN0ZWRCeSI6IiIsImFtb3VudFVTRCI6MywiYWRkcmVzc1R5cGUiOiIiLCJub3RlIjoiIiwiZXhjaGFuZ2VUeElkIjoiIiwicmVxdWVzdGVkQW1vdW50IjowLjAwMSwiZmVlQ3VycmVuY3kiOiJFVEhfVEVTVCIsIm9wZXJhdGlvbiI6IlRSQU5TRkVSIiwiY3VzdG9tZXJSZWZJZCI6bnVsbCwibnVtT2ZDb25maXJtYXRpb25zIjoxLCJhbW91bnRJbmZvIjp7ImFtb3VudCI6IjAuMDAxIiwicmVxdWVzdGVkQW1vdW50IjoiMC4wMDEiLCJuZXRBbW91bnQiOiIwLjAwMSIsImFtb3VudFVTRCI6IjMuMDAifSwiZmVlSW5mbyI6eyJuZXR3b3JrRmVlIjoiMC4wMDAwNDAzMDk5NjY5NzcifSwiZGVzdGluYXRpb25zIjpbXSwiZXh0ZXJuYWxUeElkIjpudWxsLCJibG9ja0luZm8iOnsiYmxvY2tIZWlnaHQiOiIxMTc4OTQ5NSIsImJsb2NrSGFzaCI6IjB4NzI3Njg0ODcxMWEzZDM1ODU3MzYyNDk1YmFlY2VhZGY4N2I1NzViNzdlNzE4NmE0OGFjOTc0M2NlZDQyZTMxNCJ9LCJzaWduZWRNZXNzYWdlcyI6W10sImluZGV4IjowfXx=");

            var isVerified = CryptoProvider.VerifySignature(data, signature);

            Assert.False(isVerified);
        }

        //[Test]
        //public void Test2()
        //{
        //    var signature = Convert.FromBase64String("FeIUoR1ZgkSEZ8R4uqwaoze7NtPm/vcG9W8lqk1lAcaX2/ea8tf3wyjhBLlZ9FkCVk+2ztObx/xgrqIRR9drYXlgYRql+2aIim/NO0WZu22UGQM6SCSGEgc0AQQ6WmHWry4VGsl/oQbY2ksizjaoyATJAxr4GsUHmeWuELs3a76cWNZXjSnPHyCsfMTjoI5fGSjjBou9ZZpogRWqallqTgOSQoF+nWUjNvuQPKMr1/iPcYH1VIbLVNcpgJx4rt+sVi8FFVIXMTrl3FHn0OyxpmDS/EzLwkvVKkmXXsMWE4Ip7HY+KLTydSw5z9NB7xK7w0H6whq1FuXBGe2QmTvHPN5m+i0xE9AyaZAvt/9QG+ioh+DKbPc2r0gN7cAVVO+Ayiw1QwK4ft0MRc8uTYCOK4/uQrUNUqQ8HTpHk3aqPugyDlpGlJT30KgBTs4WqYLVM4qsQ8aUdy4SXNN1m3qjuRydvL9K5Nljqb/8pdKM+pNQ2Syi4MEjumCJKj/+JPjac8/8ETNLm+MKlxehr5Mq5vlFCHEoVOK2maCzb5L/1FOh4yvI5JeBQWp7lumLuRHRZ4KycBqYp2SjG+pxlFXMOQPj8tds43bKWxQIfxz3nAiNE/1bWAX2ig/kBOowQNim38yDkc2InIqZ7JYclCwJmY+IQcKRptySIQUM3f1xJCk=");
        //    var data = Convert.FromBase64String("eyJ0eXBlIjoiVkFVTFRfQUNDT1VOVF9BRERFRCIsInRlbmFudElkIjoiZjNlYjdjYzktZjc2ZC01MDU2LWIyYmQtODE5NDc1ZWI4YjA2IiwidGltZXN0YW1wIjoxNjQyMDE0ODA1MDQ2LCJkYXRhIjp7ImlkIjoiMTkiLCJuYW1lIjoidGVzdC0zIiwiaGlkZGVuT25VSSI6ZmFsc2UsImFzc2V0cyI6W119fQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==");

        //    var isVerified = CryptoProvider.VerifySignature(data, signature);

        //    Assert.True(isVerified);
        //}

        [Test]
        public void Test3()
        {
            var signature = Convert.FromBase64String("Wd6aqEx0xV0nJ4+lf84H/GEEVV0TC/GQIy7I3ICsEoO6+PCWUSYNR2ew3LtcZLv+86JugowgcU9X8SJwRqF0H5BpkNpFtL6ThMz9ToKU+k5QkceOnD5oihB41mB+tFQKOFhghDOMEx0eAKs866wVtjJKZPJZQRr3812i2g9FCNjqKJMEkez5gjCwrxILo24BHBHSP17lxnytN78nQkRqEE197ijqceJ9QanRmordiEln0rhIUaQ+SPvoC//DCPhgpwGdqY8oaneoca6FoG6M8PqJNwsN2bshLBKI/rZnewLmbkJHeY3BkkTW+hWtliVBG9FJxIbRTfURcz0qOkB+LtxYtIt+moo7WMTTDZYgz+yrALwtfJM8AfJARwfyaLuDTakCVZ7FHV/ofWmDO4XhJmpD5GJHOLmGbzZ8/PumuzipGabUXcXvmaDBj/vM/rySLAP/eQaCPoKSlSsXwH0Q9qE4wsjMmsb2AsOfARMajCEqixSLinv7rbLCDYMmVkfohSR4QVnFnk1Y2CyRGbH2ck1x69K/ZF1PoDeTUN+uXvD8KLxUaqwJYt23g+Wycbi7YM6nQAKrjxbwZ7aJDksjXnyvAG6FTwGhYZkh987Sl+1AE5BvqosgvfKdp6P6wkwwv1DFvLydn1gQfRYaO+P3OXJZpT7hCn8caCYKYzi3Eqk=");
            var data = Convert.FromBase64String("eyJ0eXBlIjoiVkFVTFRfQUNDT1VOVF9BU1NFVF9BRERFRCIsInRlbmFudElkIjoiZjNlYjdjYzktZjc2ZC01MDU2LWIyYmQtODE5NDc1ZWI4YjA2IiwidGltZXN0YW1wIjoxNjQyNDEzNDA0NzcwLCJkYXRhIjp7ImFjY291bnRJZCI6IjE5IiwiYWNjb3VudE5hbWUiOiJ0ZXN0LTMiLCJhc3NldElkIjoiQlRDIn19");
            //"eyJ0eXBlIjoiVkFVTFRfQUNDT1VOVF9BU1NFVF9BRERFRCIsInRlbmFudElkIjoiZjNlYjdjYzktZjc2ZC01MDU2LWIyYmQtODE5NDc1ZWI4YjA2IiwidGltZXN0YW1wIjoxNjQyNDEzNDA0NzcwLCJkYXRhIjp7ImFjY291bnRJZCI6IjE5IiwiYWNjb3VudE5hbWUiOiJ0ZXN0LTMiLCJhc3NldElkIjoiQlRDIn19AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="

            var isVerified = CryptoProvider.VerifySignature(data, signature);

            Assert.True(isVerified);
        }

        [Test]
        public void Test4()
        {
            var signature = Convert.FromBase64String("w+riadembZen2QBz/6NXdm/4E1BCFVRKcQx/fCPbraZNXdzy0qsIkQC0iTNbTtPeOG/cshJDtM4lCzeZxgRrsRv/0I8KSfQGVwnotFdmL17ze9Z0A+suranaNq4RH+VwpNwDWyhcuz4sGpnQ2+2VTiHohjBNW2/qXih82SR234kHHFh72joiNp1oIXGZmguOUv/jUJWcMo4D5iHaQQVmDJenKbogSU6rg54Wa4kM5Uhrlj2JyeskX4v8EwOIAgeTHqOJuTBGDHCxuTo8g1l/bf7e7fWByIbnNoB0ao9OKev3M2i7BGx6IU/YZjCDlavDQYcRjSZ2qTcbx86pv7qN3CeKcQLBGcRRoxkW3qpqdlVfCdVr2TtyDo+l8zmjgDpkvo56n6HF8epdWPNDWaxj2FuAOoZAKRzE3BJOuUi8krdkq3rZm4EUJpMRHbkzSq1ex340DP598S6JvQF0UKAm0NO4oFxHIjrzMFY4tBT2nkKA7Xt8fj8mAnYs/S4HpqAvQHXt3vyDy0K2fY6W+G9nWtZ5V0EwjPL1rvF6XlIV2oamST/LudCR8dUmcF2f9d4mt4Rq036cFZvZQKbJsq9OQaw+r2u2ml/ihGbdY8P70kefPad/QmumDQwkD75yFsJYHnd9bKnmRLCL0tDCuN8R3RlkKfOoPXBTmW1HhNyMjUo=");
            var data = Convert.FromBase64String("eyJ0eXBlIjoiVkFVTFRfQUNDT1VOVF9BRERFRCIsInRlbmFudElkIjoiYWI1ODkwNjQtNTZmYy01YzRhLWFhOTUtMWYzMGE2ZDJhODNhIiwidGltZXN0YW1wIjoxNjQyMTUwMzU5Njc3LCJkYXRhIjp7ImlkIjoiNzEiLCJuYW1lIjoiY2xpZW50X2ZhOGYwYTU0NGY2NDRhOThiYjMwNmIxNzE1Yzc0MDMxRVRIX1RFU1QiLCJoaWRkZW5PblVJIjpmYWxzZSwiYXNzZXRzIjpbXX19AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==");

            var isVerified = CryptoProvider.VerifySignature(data, signature);

            Assert.True(isVerified);
        }
    }
}
