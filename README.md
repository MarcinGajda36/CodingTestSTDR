
1) How to run the application:
You can run this application in Visual Studio 2022 by pressing F5.
Alrernatively you can run command: "dotnet run -c Release" inside "CodingTestSTDR\CodingTestSTDR>" (the folder with "CodingTestSTDR.csproj" file)

2) any assumptions you have made:
a) Caching HackerNewsItem for 3minutes is acceptable. 
b) Configurable 17 up to concurrent request into HackerNews Api is acceptable.
c) 'beststories' return stories is descenting score order (i validated it with manual testing, but it can change in future)
d) Grabing all Kids HackerNewsItem on the way to calculating commentCount is acceptable for memory preasure.

4) any enhancements or changes you would make, given the time:
a) I would add tests.
b) I would do more streaming in HackerNewsStory, probably using IAsyncEnumerable<>, to avoid loading so much HackerNewsItem's in memory. For example on the way to grab commentCount.
c) I would considier adding specialized request to HackerNewsClient that grabs just the type of item, without carrying about all the other fileds, so commentCount is calculated quicker on cache miss.
