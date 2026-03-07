---
title: "Forecasting Errors (Part 2): Recency, relevance, seasonality"
summary: "Historical throughput only helps if it’s recent, relevant, and adjusted for seasonal cycles."
publishedAt: "2026-03-24"
type: "article"
status: "draft"
---

# Forecasting Errors (Part 2): Recency, relevance, seasonality

YT:https://youtu.be/1u4RDhNrd5s

Video: [Watch on YouTube](https://youtu.be/1u4RDhNrd5s)

Using historical throughput is good practice—until the data stops being representative of your current system.

## Three mistakes

1. Stale data (recency)
2. Irrelevant data (system change)
3. Seasonality (time-of-year cycles)

## Transcript (rough)

0:00 Hi, welcome back to part two of my series on forecasting errors.

0:08 In part one, we talked about:
- the start date being invalid
- two ways of double-dipping on throughput: (1) assuming multiple things get done at the same time, and (2) not accounting for busy/toil work required for business-as-usual.

0:23 In this set of forecasting errors I want to talk about throughput again—but specifically errors in using historical data.

## Error #4: Recency (stale throughput)

0:41 People try to impress me with 12 years of throughput history… but is it relevant?
Teams change. Tooling changes. The work changes. (We were coding in COBOL back then, and now we’re using AI to generate code.)

0:58 The historical data needs to be **recent** and **relevant**.

1:08 First is recency: don’t go back more than about **six weeks** on your team throughput.

1:12 You don’t get statistical benefit from going further back—you just introduce stale-data error.

## Error #5: Relevance (throughput from a different system)

1:20 Second is irrelevant throughput.

1:20–1:36 If you’re changing how you develop—new framework, new patterns, new architecture that can now be replicated—those factors can make older throughput data not relevant for forecasting new work.

1:44 When the team changes what it’s working on (or how it’s working), you may need to throw away historical data and go back to estimates for a period—your best guess about the new system.

1:53 This is especially true in an AI-driven world.

## Error #6: Seasonality (time of year)

2:01 There’s a third factor that affects throughput: the time of the year.

2:08 Different countries have slow and peak periods. Some companies do too (customer gatherings, off-sites, release cycles).

2:28 If your project spans December/January in Australia, you’re likely to get the wrong answer (throughput changes).

2:45 In Europe, if delivery is planned for August… you’re out of luck because many people are on holiday.

3:00 These factors have nothing to do with the work itself—just when the work happens.

3:00–3:09 Don’t assume throughput is stable throughout the year. It has cycles.
If it’s significant enough, your forecasting model should take that into account.

3:09 Thanks—this is part two of the forecasting error series. I’ll see you next time. Bye.
