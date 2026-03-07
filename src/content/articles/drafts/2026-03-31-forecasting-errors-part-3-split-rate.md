---
title: "Forecasting Errors (Part 3): Split rate (granularity mismatch)"
summary: "A single granularity mismatch between backlog items and done items can create a 2x forecasting error."
publishedAt: "2026-03-31"
type: "article"
status: "draft"
---

# Forecasting Errors (Part 3): Split rate (granularity mismatch)

YT:https://youtu.be/ajIkuoAOGys

Video: [Watch on YouTube](https://youtu.be/ajIkuoAOGys)

This one is subtle, common, and can easily create a 2x miss.

## The error

We forecast by dividing “remaining work” by “historical pace.”

But the backlog is typically made of big items (ideas/features) and the done column is typically made of smaller items (split stories/tasks).

That’s a unit mismatch.

## Transcript (rough)

0:01 Hello, welcome back. I'm Troy Magennis, and in this series we’re covering my most common forecasting errors.

0:08 This is a big one that I have trouble coaching and teaching in my workshop, but it can account for up to a 2x error in your forecasting—so pay attention.

## Error: Mixing backlog granularity with done granularity (split rate)

0:22 When we forecast work, we often take the pace we traditionally deliver and divide the remaining work by that progress.

0:40 The problem: when teams pick up work from a backlog, they often **split it into a better size to develop**.

0:49 This is logical and important, because the size of work we discuss as an idea is different from the size we need to assemble, develop, and ship—especially in an AI world.

1:04 The result: the **done column** has a lot more small-grained work than the **to-do/backlog**.

1:13 The done column is in kilometers/hour and the to-do column is in miles/hour. You can’t divide the two without introducing error.

1:30 If you don’t have another model, Troy’s rule-of-thumb is:
- ~1/3 of work doesn’t get split
- ~1/3 becomes 2 items
- ~1/3 becomes 3 items

So you might assume a split rate somewhere between 1x and 3x (varies by context).

1:44 If you’re ops/help desk where the unit in equals the unit out, there’s little/no splitting.

1:53 In complex product development with uncertainty, split rate can be much higher—one backlog item could become 10 items.

2:16 Troy notes his tools/forecasting spreadsheets include a **split rate setting** and encourages using it.

2:24 If you don’t account for splitting, it can appear like you’re moving twice as fast and completing work twice as fast as the backlog—purely because the done column is in a different granularity.

2:43 Then the team has to operate “twice as fast” just to keep pace with what the forecast said.

2:54 Don’t try to normalize away splitting. Don’t discourage teams from splitting.

3:02 Account for it in the forecast and accept that splitting is part of product development.

3:10 Benefits of splitting:
- you can defer non-important work to later releases
- you can deliver faster
- teams struggle with big pieces of work (they look almost-done for longer)

3:41 Bottom line: splitting can make you appear to be moving twice as fast, solely due to granularity mismatch.
Account for it in your forecast.

3:48 Thanks—have a great day.
