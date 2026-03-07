---
title: "Forecasting Errors (Part 1): Start date + double-dipping throughput"
summary: "Three forecasting errors that quietly break delivery confidence: fictional start dates, parallel work, and ignoring BAU/toil."
publishedAt: "2026-03-17"
type: "article"
status: "draft"
---

# Forecasting Errors (Part 1): Start date + double-dipping throughput

YT:https://youtu.be/HvcD9qEEqyc

Video: [Watch on YouTube](https://youtu.be/HvcD9qEEqyc)

This is the first in a short series of the most common forecasting errors I see in product engineering.

## The punchline

- Start dates are usually fiction.
- Throughput gets double-counted in two different ways.

If your roadmap dates keep slipping, check for these before you “fix estimation.”

## Transcript (rough)

0:00 Hi, I'm Troy Magennis and I consult on how to improve forecasting and metrics-driven development.

0:08 I want to start a series on what I've seen as the biggest forecasting errors in product engineering. I’m going to give you my top three.

## Error #1: Start date

0:16 First of all, it’s a really simple one: **start date**.

0:24 Often when we do a forecast in product engineering (or any engineering), we assume a start date where work will begin.

0:34 That hardly ever happens. If you ever see a start date that’s the beginning of a quarter or the beginning of a month—call foul.

0:41 Prior work doesn’t allow new work to start. And even if it does start, it doesn’t have a full complement of the team.

0:50 Come up with a definition of what “started” actually means for your forecasting effort and use that.

0:58 My advice is to avoid ever putting a start date in. A lot of decisions can be made in forecasting just on duration.

1:08 If we’re comparing option A vs option B, we just want to know which is longer or shorter—we don’t need a start date.

1:08–1:17 But the moment you put a start date in and a due date, an end date is going to happen in the forecasting tool and that’s what people lock onto.

1:17 If you put a start date in, make sure you understand whether you have a full team available and what might get in the way of hitting that start date.

## Errors #2 and #3: Double-dipping throughput

1:24 Errors number two and three are combined: **we double-dip on the team’s throughput capacity**.

### Error #2: Parallel work steals capacity

1:31 Often the team is doing multiple things in parallel.

1:40 When we’re asked to forecast and estimate, we do them separately: thing one, thing two, thing three.

1:47 But when we execute, we start thing one, get blocked, do a bit of thing two, move back to thing one, etc.

1:55 Neither thing one nor thing two happens in the time we expected because we’re halving (or quartering) the throughput of the team on each effort.

2:03 Make sure when you give a forecast you understand how much of the team’s capacity will be allocated to that piece of work. If it’s not 100%, don’t assume it is.

### Error #3: Ignoring BAU/toil

2:28 We assume the team is fully allocated on one piece of work, but they also have other work to do.

2:37 The backlog/done column/throughput or velocity you record includes business-as-usual day-in/day-out toil tasks.

2:44 Reserve some throughput/velocity for those tasks going forward. Don’t put 100% of the team’s historical throughput on a project.

2:53 I’ve never seen it. I start at 75%—even if the team is dedicated to a piece of work, I assume they’re only 75% allocated to that work.

3:08 If you’re dealing with a legacy platform, that could be 50% or even only 25% of a team’s effort going to new work.

3:17 This double-dipping on throughput/velocity (errors two and three) is one of the biggest problems I see in forecasting.

3:24 But don’t forget that damn start date: it’s never when you expect it, and it’s never the full complement of the team on day one.

3:33 I’ll see you next time. Thank you.
